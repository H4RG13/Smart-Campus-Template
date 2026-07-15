# Monitoring

Monitoring is scoped per school (matching the independent-deployment model) — there is no central dashboard aggregating all schools by default, because there is no shared runtime to aggregate. What exists is a lightweight, repeatable monitoring setup that gets stood up identically for every new client.

## What's monitored, per school

| Signal | Mechanism | Why it matters |
|---|---|---|
| **API liveness** | `/api/v1/health` endpoint (ASP.NET Core health checks: self, DB connectivity, and Redis/MinIO if enabled) polled by an external uptime checker | Basic "is the school's system up at all" signal |
| **Database connectivity** | Included in the `/health` check's dependency probe | A DB outage is the most likely single point of failure per school |
| **Device online/offline status** | Derived from `DEVICE_HEARTBEATS` (last-seen timestamp) — a device with no heartbeat in 3x its expected interval flags as offline on the `/devices` dashboard | Directly affects whether attendance is actually being captured that day — this is the signal school staff care about most day-to-day |
| **SSL certificate expiry** | Certbot's own renewal + a simple expiry check in the uptime monitor | Silent SSL expiry is a common, entirely preventable outage cause |
| **Disk space (Postgres/MinIO volumes)** | Host-level check (e.g. a simple cron script or the VPS provider's built-in alerting) | Attendance/audit data grows continuously; running out of disk is a slow-building, easily-missed failure |
| **Notification delivery failures** | `NOTIFICATION_LOGS.Status = Failed` surfaced on an admin-facing view, not just buried in logs | An SMTP misconfiguration or credential expiry otherwise fails silently from the school's perspective |

## Tooling choice

**External uptime checker** (e.g. a simple third-party HTTP uptime monitor, or a lightweight self-hosted one like Uptime Kuma run centrally by the agency, polling each school's public `/health` endpoint) — deliberately not a heavyweight observability platform (no Datadog/New Relic agent per school). Rationale: at this scale, "is it up, and is the DB reachable" answers the overwhelming majority of real incidents; anything requiring deep APM tracing is disproportionate to a single-school admin tool's traffic volume.

**Why one central uptime checker is the acceptable exception to "nothing shared":** it only *polls* public health endpoints from outside — it holds no credentials, is not a runtime dependency of any school's system (a school functions identically whether or not the checker itself is up), and its failure affects alerting visibility only, not school operations. This is analogous to the off-box backup destination noted in `DEPLOYMENT_ARCHITECTURE.md`.

## Alerting

- **Uptime checker → agency's on-call channel** (e.g. email/Slack webhook) when a school's `/health` fails or responds unhealthy for more than N consecutive checks (avoid alert noise from a single transient blip).
- **Device offline alert** — surfaced in-app on the `/devices` dashboard for school staff to see directly (they're best positioned to know "oh, that reader's power got unplugged"); a backend-triggered email to the school's admin contact if a device has been offline more than a configurable threshold (e.g. 2 hours during school hours), since staff may not check the dashboard proactively.
- **SSL expiry / disk space** → agency's on-call channel only (operational, not school-facing).

## What's explicitly out of scope for V1

- No distributed tracing / APM — a modular monolith with no cross-service calls has little to trace; Serilog's structured request logging (see `LOGGING.md`) covers the practical debugging need.
- No centralized cross-school metrics dashboard — would imply shared infrastructure the business model explicitly avoids; if the agency later wants a "fleet health" view across clients, that's a separate, clearly-optional tool that only *reads* each school's public health/status endpoints, never a dependency of any school's runtime.
- No synthetic transaction monitoring (e.g. scripted login + click-through checks) — health endpoint + device heartbeat cover the realistic failure modes at this scale; add if a specific incident demonstrates the gap (per Rule #8's "no infrastructure without an observed need").
