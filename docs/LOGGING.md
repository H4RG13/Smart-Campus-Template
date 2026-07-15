# Logging

Serilog, structured (JSON) logging, per-school (each school's `api` container writes its own logs — no cross-school log aggregation, same rationale as `MONITORING.md`).

## Distinction: application logs vs. audit trail

These are two different systems and must not be conflated:

- **Application logs (Serilog)** — operational: request/response timing, exceptions, startup diagnostics, device ingestion processing outcomes. Answers "why did the system behave this way / what broke." Rotated and eventually discarded.
- **Audit trail (`AUDIT_LOGS` table, see `DATABASE_DESIGN.md`)** — business record: who changed what entity, when, with a diff. Answers "who marked this student absent" or "who edited this attendance record" for accountability purposes. Retained indefinitely (it's school operational history, not a debugging artifact) and never rotated away.

Mixing these (e.g. trying to reconstruct "who changed what" from log files) is exactly the kind of fragile pattern this separation avoids — the audit trail is queryable, structured, permanent; logs are ephemeral and optimized for operators, not compliance.

## Log levels and what goes where

| Level | Used for | Example |
|---|---|---|
| `Debug` | Verbose diagnostic detail, disabled in production by default | Full request payload on a specific troubleshooting flag |
| `Information` | Normal operational events worth knowing happened | "Device {DeviceId} event {DeviceEventId} processed, status={Status}", "User {UserId} logged in" |
| `Warning` | Recoverable but noteworthy — the system handled it, but a human might want to know | Duplicate device event rejected, notification retry, device heartbeat late |
| `Error` | Unhandled exception, failed operation that needed intervention | Unhandled exception in a controller, SMTP send failure, DB connection failure |
| `Fatal` | Startup failure, unrecoverable state | Config validation failure at boot (fail-fast per Rule #3) |

**Production default:** `Information` and above to the console/file sink; `Debug` available via a runtime-toggleable minimum level (Serilog's `LoggingLevelSwitch`) for temporary deep troubleshooting without a redeploy.

## Structured logging conventions

- Always log with named properties, never string-interpolate values into the message template — `Log.Information("Attendance recorded for {StudentId} with status {Status}", studentId, status)`, not `Log.Information($"Attendance recorded for {studentId}...")`. This keeps logs queryable/filterable by field once ingested anywhere (even a simple `grep`/`jq` over JSON lines benefits).
- Every HTTP request gets a correlation ID (Serilog's `RequestLogging` middleware or a custom one), included on every log line emitted during that request's handling — makes tracing "everything that happened processing this one device event" straightforward without a distributed tracing system.
- Never log secrets or PII beyond what's operationally necessary — no password hashes, no full JWTs, no RFID tag IDs in `Information`-level logs by default (tag ID is fine in `Debug` for troubleshooting a specific device issue, gated behind the debug level switch).

## Sinks

- **Console** — always, for `docker compose logs` / `docker logs` accessibility during day-to-day operation and incident response.
- **Rolling file** — a local file sink with daily rolling + retention (e.g. 30 days), so logs survive container restarts and give a debugging window without needing external infrastructure.
- **No external log aggregation service by default** (no shipping to a centralized ELK/Datadog/etc. per school) — same reasoning as `MONITORING.md`: disproportionate to a single-school admin tool's log volume. If a specific school's support burden justifies it later, add a sink for that school only, not a template-wide default (Rule #8).

## Retention

- **Application logs:** 30 days rolling, then discarded — they're for near-term operational debugging, not historical record.
- **Audit trail:** indefinite — it's data, not logs (see distinction above); subject to the same backup strategy as the rest of the database (`DEPLOYMENT_ARCHITECTURE.md`).

## What's explicitly out of scope for V1

- No log-based alerting pipeline (e.g. alerting on error-log volume) — the health-check and heartbeat-based alerting in `MONITORING.md` covers the practical incident signals at this scale; revisit only if a real incident shows a gap logs would have caught earlier.
- No PII scrubbing pipeline beyond the "don't log it in the first place" discipline above — a dedicated scrubbing layer is unneeded complexity when the simpler rule (never emit PII into `Information`+ logs) is enforced at the call site.
