# ADR 0001 — Async request-reply: HTTP in, WebSocket out

- **Status:** Accepted
- **Date:** 2026-09-05
- **Applies to:** `backend/`, `frontend/`, later `mobile/`

## Context

Some operations (opportunity matching, impact-report generation, exports) are
too slow to hold an HTTP request open for. We want the client to submit work
over HTTP and receive the result over a persistent socket, so the request
thread is released immediately and the UI stays responsive.

## Decision

Adopt the **async request-reply** pattern with a **job ID + caller connection**
correlation:

```
1.  Client opens        GET /ws                    (WebSocket upgrade)
2.  Server replies      { type: "connection.established", connectionId }
3.  Client submits      POST /api/jobs/opportunity-search
                        { connectionId, query }
4.  Server replies      202 Accepted { jobId }     (HTTP request ends here)
5.  Worker pushes       { type: "job.running",   jobId }
6.  Worker pushes       { type: "job.completed", jobId, result }
                   or   { type: "job.failed",    jobId, error }
7.  Client matches the message to its pending request by jobId.
```

### Transport: raw WebSockets, not SignalR

Chosen deliberately over SignalR to avoid the client dependency and keep the
wire format explicit and inspectable. The cost is that we own the pieces
SignalR would have provided:

| Concern | How we handle it |
|---|---|
| Message framing | One JSON envelope, `SocketMessage`, discriminated by `type`. |
| Connection registry | `IConnectionRegistry` — in-memory `ConcurrentDictionary`. |
| Concurrent sends | `SemaphoreSlim` per connection. `WebSocket.SendAsync` is **not** safe for concurrent callers. |
| Keepalive | `WebSocketOptions.KeepAliveInterval` (30s) sends protocol-level ping frames; browsers auto-pong. |
| Reconnect | Client-side exponential backoff with jitter, capped at 30s. |

### Correlation: job ID + caller connection

The result is pushed to the one connection that submitted the job. This works
today with no authentication, which JWT-based user groups would have required
(still an open Phase 0 item in `docs/plan.md`).

**Accepted limitation:** if the client disconnects between submitting and
completion, the result is dropped. The server logs it and moves on. The client
gets a timeout, not a wrong answer.

### Execution: in-process queue, single instance

`Channel<JobWorkItem>` feeding a `BackgroundService`. Each job runs in its own
DI scope and is dispatched through MediatR, so **any** existing command or
query can be run as a background job without new plumbing.

## Consequences

### What this rules in

- Jobs carry `IBaseRequest`, so `ISender.Send(object)` dispatches them. Adding a
  new job type means adding a MediatR request — no changes to the transport.
- The wire format is plain JSON and readable in browser devtools.

### What breaks if we scale past one API instance

The connection registry and the job queue are both in-process. With two
replicas behind a load balancer, a job submitted to replica A cannot notify a
client connected to replica B. To fix, in rough order of effort:

1. Sticky sessions at the load balancer (stopgap; breaks on redeploy).
2. Redis pub/sub backplane — replicas publish notifications, each replica
   delivers to its own local connections. The `redis` service is already in the
   `docker-compose` task in `docs/plan.md`.
3. A durable queue (and job store) so work survives a restart.

Today an API restart loses every queued job. That is acceptable while jobs are
read-only searches; it is **not** acceptable once a job mutates data.

### Security

- `WebSocketOptions.AllowedOrigins` is set from the same configured origin list
  as CORS. Browsers do **not** apply CORS to WebSocket handshakes, so without
  this any site could open a socket to the API — cross-site WebSocket hijacking.
- Connection IDs are server-generated GUIDs, never client-supplied.
- A submit whose `connectionId` is unknown or closed is rejected 400 by a
  FluentValidation rule rather than silently queued.

## Measured limits

Stress-tested 2026-09-05 (`scripts/stress/`, full baseline in its README).
Debug build, single instance, localhost, 1,200 ms simulated repository latency.

| Limit | Measured |
|---|---|
| Concurrent sockets | 2,000 with zero failures — ceiling not found |
| Job latency with 2,000 sockets held | 1,214 ms, i.e. undegraded |
| **Job throughput** | **0.82 jobs/sec** |
| Queue accepted before blocking | 101 submissions, p50 2 ms |
| Submits after the queue filled | p50 1,211 ms, **0 rejected** |
| Misrouted results / unhandled exceptions | 0 |

Two findings worth acting on:

1. **Job concurrency is 1.** `JobProcessorService` awaits each job inside its
   `await foreach`, so throughput is `workers / jobDuration`. The queue is global:
   30 clients submitting one job each took the same 36 s as one client submitting
   30. Adding users does not add capacity.

2. **The client budget is breached at 25 concurrent jobs.**
   `clientBudget x workers / jobDuration` = `30 s x 1 / 1.2 s`. Beyond that, users
   see timeouts while the server is healthy and still working. Measured: 6 of 30
   jobs exceeded the budget.

The fix is small — a bounded parallel loop in the worker plus `SingleReader =
false` on the channel — and scales the threshold linearly with worker count.
Left undone deliberately: it is not a transport change, and it should be
measured against real work rather than a `Task.Delay` that parallelises
perfectly.

## Alternatives rejected

| Option | Why not |
|---|---|
| SignalR | Would have handled reconnect, backplane, and framing for us. Rejected for the extra dependency; revisit if the hand-written client becomes a maintenance burden. |
| Server-Sent Events | Simpler and auto-reconnecting, but one-directional and capped at 6 connections per origin on HTTP/1.1. |
| HTTP polling on `GET /api/jobs/{id}` | Considered as a recovery path for dropped results. Not built — revisit if dropped results become a real complaint. |
| Long-polling | Strictly worse than the socket we already need. |
