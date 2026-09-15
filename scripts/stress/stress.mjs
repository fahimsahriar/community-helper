#!/usr/bin/env node
/**
 * Stress harness for the async request-reply channel (docs/adr/0001).
 *
 * Measures the limits the ADR asserts but never proved: socket capacity,
 * job throughput, queue backpressure, and result routing under concurrency.
 *
 * Usage:  node scripts/stress/stress.mjs <scenario> [options]
 * See:    node scripts/stress/stress.mjs --help
 *
 * Requires Node 22+ (global WebSocket and fetch).
 */

const API = process.env.STRESS_API ?? 'http://localhost:5000';
const WS = process.env.STRESS_WS ?? 'ws://localhost:5000/ws';

/** Generous on purpose: we want to observe slow jobs, not give up on them. */
const HARNESS_TIMEOUT_MS = 180_000;

/** What the Angular client actually allows (JOB_TIMEOUT_MS in the frontend). */
const CLIENT_BUDGET_MS = 30_000;

// ---------------------------------------------------------------- statistics

const percentile = (sorted, p) => {
  if (sorted.length === 0) return NaN;
  const rank = Math.ceil((p / 100) * sorted.length) - 1;
  return sorted[Math.min(sorted.length - 1, Math.max(0, rank))];
};

function summarize(values) {
  const sorted = [...values].sort((a, b) => a - b);
  return {
    n: sorted.length,
    min: sorted[0] ?? NaN,
    p50: percentile(sorted, 50),
    p95: percentile(sorted, 95),
    p99: percentile(sorted, 99),
    max: sorted.at(-1) ?? NaN,
  };
}

const ms = (v) => (Number.isFinite(v) ? `${Math.round(v)}` : '—');

function printStats(label, values) {
  const s = summarize(values);
  console.log(
    `  ${label.padEnd(22)} n=${String(s.n).padStart(4)}  ` +
      `min=${ms(s.min).padStart(6)}  p50=${ms(s.p50).padStart(6)}  ` +
      `p95=${ms(s.p95).padStart(6)}  p99=${ms(s.p99).padStart(6)}  max=${ms(s.max).padStart(7)}  (ms)`,
  );
}

function section(title) {
  console.log(`\n${'━'.repeat(78)}\n${title}\n${'━'.repeat(78)}`);
}

// -------------------------------------------------------------------- client

/**
 * One socket connection plus the job bookkeeping around it.
 *
 * Buffers messages that arrive before their job id is known — the same
 * ordering hazard the Angular client solves with a replay buffer. Without
 * this, fast jobs are silently lost.
 */
class StressClient {
  #socket = null;
  #pending = new Map(); // jobId -> { resolve, reject, timer }
  #early = new Map(); // jobId -> message[] seen before submit() resolved

  connectionId = null;
  foreignMessages = 0; // messages for jobs this client never submitted
  closedUnexpectedly = false;

  connect() {
    const startedAt = performance.now();

    return new Promise((resolve, reject) => {
      let socket;
      try {
        socket = new WebSocket(WS);
      } catch (err) {
        reject(err);
        return;
      }

      this.#socket = socket;

      const failFast = setTimeout(
        () => reject(new Error('handshake timed out after 15s')),
        15_000,
      );

      socket.addEventListener('message', (event) => {
        const message = JSON.parse(event.data);

        if (message.type === 'connection.established') {
          this.connectionId = message.connectionId;
          clearTimeout(failFast);
          resolve(performance.now() - startedAt);
          return;
        }

        this.#route(message);
      });

      socket.addEventListener('error', () => {
        clearTimeout(failFast);
        reject(new Error('socket error'));
      });

      socket.addEventListener('close', () => {
        clearTimeout(failFast);
        if (this.#pending.size > 0) this.closedUnexpectedly = true;
      });
    });
  }

  #route(message) {
    const { jobId } = message;
    if (!jobId) return;

    const waiter = this.#pending.get(jobId);
    if (waiter) {
      this.#settle(jobId, waiter, message);
      return;
    }

    // Either the result beat the 202, or it belongs to someone else.
    // We cannot tell them apart yet, so hold it briefly.
    const bucket = this.#early.get(jobId) ?? [];
    bucket.push(message);
    this.#early.set(jobId, bucket);
  }

  #settle(jobId, waiter, message) {
    if (message.type === 'job.running') return; // progress, not an outcome

    clearTimeout(waiter.timer);
    this.#pending.delete(jobId);
    this.#early.delete(jobId);

    if (message.type === 'job.completed') {
      waiter.resolve({ results: message.result ?? [] });
    } else if (message.type === 'job.failed') {
      waiter.reject(new Error(message.error ?? 'job failed'));
    }
  }

  /** Submits a job and resolves when its result arrives. */
  async submit(query, { timeoutMs = HARNESS_TIMEOUT_MS } = {}) {
    const submitStart = performance.now();

    const response = await fetch(`${API}/api/jobs/opportunity-search`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ connectionId: this.connectionId, query }),
    });

    const submitMs = performance.now() - submitStart;

    if (response.status !== 202) {
      const body = await response.text();
      throw Object.assign(new Error(`submit ${response.status}: ${body.slice(0, 160)}`), {
        submitMs,
        rejected: true,
      });
    }

    const { jobId } = await response.json();

    const settled = new Promise((resolve, reject) => {
      const timer = setTimeout(() => {
        this.#pending.delete(jobId);
        reject(Object.assign(new Error('job timed out'), { timedOut: true }));
      }, timeoutMs);

      this.#pending.set(jobId, { resolve, reject, timer });
    });

    // Drain anything that arrived before we knew the job id.
    for (const message of this.#early.get(jobId) ?? []) {
      this.#route(message);
    }

    const outcome = await settled;
    return {
      ...outcome,
      submitMs,
      totalMs: performance.now() - submitStart,
    };
  }

  /** Counts buffered messages that never matched a job this client submitted. */
  auditForeignMessages() {
    for (const [, messages] of this.#early) this.foreignMessages += messages.length;
    return this.foreignMessages;
  }

  close() {
    for (const [, waiter] of this.#pending) clearTimeout(waiter.timer);
    this.#pending.clear();
    try {
      this.#socket?.close();
    } catch {
      /* already gone */
    }
  }
}

// ------------------------------------------------------------------ scenarios

/** How many sockets the server will hold, and what connecting costs as it fills. */
async function scenarioConnections({ target, batch }) {
  section(`SOCKET CAPACITY · ramping to ${target} concurrent connections`);
  console.log(
    `  Capped at ${target} on purpose: this runs on a dev machine, and exhausting\n` +
      `  ephemeral ports would disrupt the host rather than teach us anything.\n`,
  );

  const clients = [];
  const connectTimes = [];
  let failed = 0;
  let firstFailure = null;

  while (clients.length < target && failed === 0) {
    const size = Math.min(batch, target - clients.length);
    const wave = Array.from({ length: size }, () => new StressClient());

    const settled = await Promise.allSettled(wave.map((c) => c.connect()));

    settled.forEach((outcome, i) => {
      if (outcome.status === 'fulfilled') {
        connectTimes.push(outcome.value);
        clients.push(wave[i]);
      } else {
        failed++;
        firstFailure ??= `${outcome.reason?.message} (at ~${clients.length + i} open)`;
      }
    });

    process.stdout.write(`\r  open: ${clients.length}   failed: ${failed}   `);
  }

  console.log('\n');
  printStats('connect latency', connectTimes);

  // Does the server still work with all those sockets held open?
  const probe = new StressClient();
  await probe.connect();
  const probeStart = performance.now();
  await probe.submit('tutoring');
  const probeMs = performance.now() - probeStart;
  probe.close();

  console.log(`\n  Held open ...............  ${clients.length}`);
  console.log(`  Failed to connect .......  ${failed}${firstFailure ? ` — ${firstFailure}` : ''}`);
  console.log(`  Job latency while held ..  ${Math.round(probeMs)} ms`);

  for (const c of clients) c.close();
  return { held: clients.length, failed, probeMs };
}

/** Throughput ceiling: fire N jobs at once from one connection. */
async function scenarioThroughput({ jobs }) {
  section(`THROUGHPUT · ${jobs} jobs submitted at once from one connection`);

  const client = new StressClient();
  await client.connect();

  const started = performance.now();
  const settled = await Promise.allSettled(
    Array.from({ length: jobs }, () => client.submit('tutoring')),
  );
  const wallMs = performance.now() - started;

  const ok = settled.filter((s) => s.status === 'fulfilled').map((s) => s.value);
  const failures = settled.filter((s) => s.status === 'rejected');
  const timeouts = failures.filter((f) => f.reason?.timedOut).length;

  const overBudget = ok.filter((r) => r.totalMs > CLIENT_BUDGET_MS).length;

  printStats('submit → 202', ok.map((r) => r.submitMs));
  printStats('end to end', ok.map((r) => r.totalMs));

  console.log(`\n  Completed ...............  ${ok.length} / ${jobs}`);
  console.log(`  Failed ..................  ${failures.length} (${timeouts} timed out)`);
  console.log(`  Wall clock ..............  ${(wallMs / 1000).toFixed(1)} s`);
  console.log(`  Throughput ..............  ${(ok.length / (wallMs / 1000)).toFixed(2)} jobs/sec`);
  console.log(
    `  Past the ${CLIENT_BUDGET_MS / 1000}s client budget  ${overBudget} / ${ok.length}` +
      `  ← these would fail in the browser`,
  );

  client.auditForeignMessages();
  client.close();
  return { ok: ok.length, wallMs, overBudget, throughput: ok.length / (wallMs / 1000) };
}

/** Does a full queue apply backpressure, or does it drop / error? */
async function scenarioBackpressure({ jobs }) {
  section(`BACKPRESSURE · ${jobs} submissions against a 100-slot queue`);

  const client = new StressClient();
  await client.connect();

  const submitTimes = [];
  let rejected = 0;

  // Sequential submits: we want each 202's latency, not aggregate throughput.
  for (let i = 0; i < jobs; i++) {
    const started = performance.now();
    try {
      const response = await fetch(`${API}/api/jobs/opportunity-search`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ connectionId: client.connectionId, query: 'tutoring' }),
      });
      submitTimes.push({ i, ms: performance.now() - started, status: response.status });
      if (response.status !== 202) rejected++;
      await response.arrayBuffer();
    } catch (err) {
      submitTimes.push({ i, ms: performance.now() - started, status: `ERR ${err.message}` });
      rejected++;
    }
    process.stdout.write(`\r  submitted: ${i + 1}/${jobs}   `);
  }

  console.log('\n');

  const fast = submitTimes.filter((s) => s.ms < 100);
  const slow = submitTimes.filter((s) => s.ms >= 100);

  printStats('all submits', submitTimes.map((s) => s.ms));
  if (fast.length) printStats('fast (<100ms)', fast.map((s) => s.ms));
  if (slow.length) printStats('blocked (>=100ms)', slow.map((s) => s.ms));

  console.log(`\n  Accepted (202) ..........  ${jobs - rejected} / ${jobs}`);
  console.log(`  Rejected / errored ......  ${rejected}`);
  console.log(`  Fast submits ............  ${fast.length}  (queue had room)`);
  console.log(`  Blocked submits .........  ${slow.length}  (queue full — waiting for a slot)`);

  if (slow.length > 0) {
    console.log(`  Backpressure began at ...  submission #${slow[0].i + 1}`);
  }

  client.close();
  return { rejected, fastCount: fast.length, blockedAt: slow[0]?.i ?? null };
}

/** Many clients at once: does every result reach the right socket? */
async function scenarioFanout({ clients: clientCount, jobsEach }) {
  section(`FAN-OUT · ${clientCount} connections × ${jobsEach} job(s) each`);

  const clients = Array.from({ length: clientCount }, () => new StressClient());
  await Promise.all(clients.map((c) => c.connect()));

  const ids = new Set(clients.map((c) => c.connectionId));
  console.log(`  Distinct connection ids .  ${ids.size} / ${clientCount}`);

  const started = performance.now();
  const settled = await Promise.allSettled(
    clients.flatMap((c) =>
      Array.from({ length: jobsEach }, () => c.submit('bangladesh')),
    ),
  );
  const wallMs = performance.now() - started;

  const ok = settled.filter((s) => s.status === 'fulfilled').map((s) => s.value);
  const failures = settled.filter((s) => s.status === 'rejected');

  const wrongPayload = ok.filter((r) => r.results.length !== 2).length;
  const crossTalk = clients.reduce((sum, c) => sum + c.auditForeignMessages(), 0);
  const droppedSockets = clients.filter((c) => c.closedUnexpectedly).length;

  printStats('end to end', ok.map((r) => r.totalMs));

  console.log(`\n  Completed ...............  ${ok.length} / ${clientCount * jobsEach}`);
  console.log(`  Failed ..................  ${failures.length}`);
  console.log(`  Wall clock ..............  ${(wallMs / 1000).toFixed(1)} s`);
  console.log(`  Wrong result payload ....  ${wrongPayload}   ← must be 0`);
  console.log(`  Messages on wrong socket   ${crossTalk}   ← must be 0`);
  console.log(`  Sockets dropped mid-job .  ${droppedSockets}   ← must be 0`);

  for (const c of clients) c.close();
  return { ok: ok.length, wrongPayload, crossTalk, droppedSockets };
}

// ----------------------------------------------------------------------- main

const HELP = `
Stress harness — async request-reply channel

  node scripts/stress/stress.mjs <scenario> [options]

Scenarios
  connections   Ramp concurrent WebSocket connections and measure connect cost.
  throughput    Fire N jobs at once; measure the job/sec ceiling.
  backpressure  Submit past the 100-slot queue; prove submits wait, not fail.
  fanout        Many clients at once; assert results route to the right socket.
  all           Run every scenario in order.

Options
  --jobs=N       jobs for throughput / backpressure   (default 30 / 130)
  --target=N     socket target for connections        (default 400)
  --batch=N      connections opened per wave          (default 25)
  --clients=N    clients for fanout                   (default 20)
  --each=N       jobs per client for fanout           (default 1)

Environment
  STRESS_API     default ${API}
  STRESS_WS      default ${WS}
`;

function parseArgs(argv) {
  const opts = {};
  for (const arg of argv) {
    const match = /^--([a-z]+)=(.+)$/.exec(arg);
    if (match) opts[match[1]] = Number(match[2]);
  }
  return opts;
}

async function waitForApi() {
  for (let i = 0; i < 30; i++) {
    try {
      const res = await fetch(`${API}/health`);
      if (res.ok) return true;
    } catch {
      /* not up yet */
    }
    await new Promise((r) => setTimeout(r, 1000));
  }
  return false;
}

async function main() {
  const [scenario = 'all', ...rest] = process.argv.slice(2);

  if (scenario === '--help' || scenario === '-h') {
    console.log(HELP);
    return;
  }

  if (!(await waitForApi())) {
    console.error(`\nCannot reach ${API}/health — start the API first:\n`);
    console.error('  cd backend && dotnet run --project src/CommunityHelper.API --launch-profile http\n');
    process.exitCode = 1;
    return;
  }

  const opts = parseArgs(rest);
  console.log(`\nTarget: ${API}  (socket ${WS})`);
  console.log(`Client budget for comparison: ${CLIENT_BUDGET_MS / 1000}s\n`);

  const run = {
    connections: () =>
      scenarioConnections({ target: opts.target ?? 400, batch: opts.batch ?? 25 }),
    throughput: () => scenarioThroughput({ jobs: opts.jobs ?? 30 }),
    backpressure: () => scenarioBackpressure({ jobs: opts.jobs ?? 130 }),
    fanout: () =>
      scenarioFanout({ clients: opts.clients ?? 20, jobsEach: opts.each ?? 1 }),
  };

  const order = scenario === 'all' ? Object.keys(run) : [scenario];

  for (const name of order) {
    if (!run[name]) {
      console.error(`Unknown scenario "${name}".${HELP}`);
      process.exitCode = 1;
      return;
    }
    await run[name]();
  }

  console.log('\nDone.\n');
}

main().then(
  () => process.exit(process.exitCode ?? 0),
  (err) => {
    console.error('\nHarness failed:', err);
    process.exit(1);
  },
);
