# Jobs API and realtime socket

Async request-reply: work is submitted over HTTP and the result is pushed over a
WebSocket. The design and its trade-offs are in
[ADR 0001](../adr/0001-async-request-reply-over-websockets.md).

Base URL (development): `http://localhost:5000`

---

## `GET /ws` — realtime socket

WebSocket upgrade. A plain HTTP `GET` returns `400 Bad Request`.

Origin is checked against the configured `Cors:AllowedOrigins` list. Browsers do
not apply CORS to WebSocket handshakes, so a disallowed origin is rejected at
the handshake rather than by the browser.

The server sends the handshake message immediately on connect:

```json
{ "type": "connection.established", "connectionId": "ed5ebf9f8fef4ed3a9e4b7e960ee8fa3" }
```

Clients send nothing. Keepalive is handled by protocol-level ping frames every
30 seconds; browsers reply automatically.

### Message envelope

Every server-to-client message shares one shape, discriminated by `type`.
Absent fields are omitted rather than sent as `null`.

| Field | Type | Present on |
|---|---|---|
| `type` | `string` | always |
| `connectionId` | `string` | `connection.established` |
| `jobId` | `string` | all `job.*` messages |
| `result` | `unknown` | `job.completed` |
| `error` | `string` | `job.failed` |

| `type` | Meaning |
|---|---|
| `connection.established` | Handshake. Carries the id needed to submit jobs. |
| `job.running` | The worker picked the job up. |
| `job.completed` | Finished. `result` holds the handler's return value. |
| `job.failed` | Failed. `error` is safe to show a user; internals are stripped in production. |

**Ordering caveat:** `job.running` frequently arrives *before* the HTTP `202`
that carries the `jobId`, because the two travel on different connections. A
client must buffer recent messages, or it will miss its own result. The Angular
client keeps a 60-second replay buffer for exactly this reason.

---

## `POST /api/jobs/opportunity-search`

Queues an opportunity search and returns immediately.

### Request

```json
{ "connectionId": "ed5ebf9f8fef4ed3a9e4b7e960ee8fa3", "query": "tutoring" }
```

| Field | Rules |
|---|---|
| `connectionId` | Required. Must be a currently open socket connection. |
| `query` | Optional, max 200 characters. Blank matches everything. |

### Response `202 Accepted`

```json
{ "jobId": "6bec35d885b24f7585e1c7c1a187bbc0" }
```

### Response `400 Bad Request`

Problem Details with an `errors` map — for example, an unknown or closed connection:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "instance": "/api/jobs/opportunity-search",
  "errors": {
    "ConnectionId": ["Unknown or closed socket connection. Reconnect and try again."]
  },
  "traceId": "0HNOBAA70BM94:00000001"
}
```

### Result payload

`job.completed` carries the same `OpportunityDto[]` documented in
[opportunities.md](./opportunities.md), ordered soonest-first.

---

## Full exchange

```
client                                         server
  |  GET /ws  (upgrade)                          |
  |--------------------------------------------->|
  |<--- { type: connection.established, ... } ----|
  |                                              |
  |  POST /api/jobs/opportunity-search           |
  |--------------------------------------------->|
  |<--- { type: job.running, jobId } ------------|   (may precede the 202)
  |<--- 202 { jobId } ---------------------------|
  |                                              |
  |<--- { type: job.completed, jobId, result } --|
```

---

## Adding another job type

No transport changes are needed. Jobs carry a MediatR request, dispatched with
`ISender.Send(object)`:

1. Add the query or command that does the work.
2. Add a `Submit…Command` whose handler enqueues a `JobWorkItem`.
3. Add a validator that checks `connectionId` against `IClientConnectionRegistry`.
4. Add the controller action returning `202` with the job id.

---

## Known limitations

- **Results are dropped if the client disconnects mid-job.** The server logs it;
  the client times out after 30 seconds. There is no `GET /api/jobs/{id}`
  recovery endpoint.
- **Single instance only.** The connection registry and job queue are both
  in-process.
- **Queued jobs do not survive a restart.** Acceptable while jobs are read-only.
