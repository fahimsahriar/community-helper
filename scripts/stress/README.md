# Stress harness — async request-reply channel

Measures the limits [ADR 0001](../../docs/adr/0001-async-request-reply-over-websockets.md)
asserts but never proved.

## Running it

Start the API, then run a scenario:

```bash
cd backend && dotnet run --project src/CommunityHelper.API --launch-profile http
```

```bash
node scripts/stress/stress.mjs all
```

| Scenario | Question it answers |
|---|---|
| `connections` | How many sockets can we hold, and what does connecting cost as it fills? |
| `throughput` | How many jobs per second, and how many blow the client's 30 s budget? |
| `backpressure` | Does a full queue wait, or drop and error? |
| `fanout` | Under concurrency, does every result reach the socket that asked for it? |

Options: `--jobs=N`, `--target=N`, `--batch=N`, `--clients=N`, `--each=N`.
Point it elsewhere with `STRESS_API` / `STRESS_WS`. Needs Node 22+.

The `connections` scenario is deliberately capped. Exhausting the host's
ephemeral ports would disrupt the machine rather than teach us anything.

---

## Baseline — 5 September 2026

Debug build, single instance on `localhost`, Windows 11, Node 24. The
`InMemoryOpportunityRepository` carries a **1,200 ms simulated latency** standing
in for the PRD's matching engine; it dominates every job measurement below.

### The connection layer is not the constraint

| Measure | 400 sockets | 2,000 sockets |
|---|---|---|
| Failed to connect | 0 | 0 |
| Connect latency p50 | 33 ms | 131 ms |
| Connect latency p99 | 56 ms | 191 ms |
| Job latency while held | 1,219 ms | 1,214 ms |

Connect cost grows with the fleet, but a job running alongside 2,000 open
sockets takes the same time as one running alongside none. We never found the
socket ceiling.

### The worker is the constraint

30 jobs submitted at once from one connection:

| Measure | Value |
|---|---|
| Submit → 202 (p50) | 305 ms |
| End to end (p50) | 18,494 ms |
| End to end (max) | 36,636 ms |
| Wall clock | 36.7 s |
| **Throughput** | **0.82 jobs/sec** |
| Past the 30 s client budget | **6 of 30** |

`JobProcessorService` awaits each job inside its `await foreach`, so job
concurrency is exactly 1. Throughput is therefore:

```
throughput = workers / jobDuration = 1 / 1.2 s = 0.83 jobs/sec
```

Measured 0.82. The model holds.

**The queue is global, not per-client.** 30 separate connections submitting one
job each took 36.3 s — the same as 30 jobs from one connection. Adding users
does not add capacity.

### How many concurrent jobs before users see failures

```
maxConcurrent = clientBudget x workers / jobDuration
              = 30 s x 1 / 1.2 s
              = 25 jobs
```

Job 25 of a simultaneous burst is the first to exceed the Angular client's
30-second timeout. Confirmed twice: 6 of 30 over budget in `throughput`, the
same shape in `fanout`.

### Backpressure behaves as designed

130 sequential submissions against the 100-slot queue:

| Measure | Value |
|---|---|
| Accepted (202) | 130 / 130 |
| Rejected or errored | **0** |
| Fast submits (queue had room) | 101, p50 **2 ms** |
| Blocked submits (queue full) | 29, p50 **1,211 ms** |
| Backpressure began at | submission **#102** |

101 rather than 100 because the worker had already dequeued one. Past that,
each submit waits exactly one job duration for a slot — the HTTP caller absorbs
the overload instead of the heap. `FullMode.Wait` does what the ADR claimed.

### Correctness held throughout

| Assertion | Result |
|---|---|
| Distinct connection ids | 30 / 30 |
| Results delivered to the wrong socket | **0** |
| Wrong result payload | **0** |
| Sockets dropped mid-job | **0** |
| Unhandled exceptions, whole run | **0** |

Disconnecting with ~100 jobs still queued produced 24 `Dropped …: connection is
gone` warnings and no errors — the documented limitation, behaving as
documented.

---

## What this says to do next

**Raise worker concurrency.** This is the single highest-value change, and it is
small. In `JobProcessorService.ExecuteAsync`, replace the sequential loop with a
bounded parallel one, and flip `SingleReader` to `false` in `BackgroundJobQueue`
— the channel currently skips reader-side synchronisation because it trusts
there is exactly one consumer.

At 8 workers the model predicts ~6.6 jobs/sec and a timeout threshold near 200
concurrent jobs, an 8x improvement for a handful of lines. Re-run `throughput`
to confirm rather than assuming: `Task.Delay` parallelises perfectly, and real
work competing for CPU or a database connection pool will not.

**Then reconsider the ordering.** Nothing today guarantees jobs finish in
submission order, and parallel workers make out-of-order completion routine.
Fine for searches. Not fine if a job ever mutates data.

**The socket ceiling is still unknown.** 2,000 was comfortable. Worth finding
the real number on a machine that resembles production before sizing anything
on it.
