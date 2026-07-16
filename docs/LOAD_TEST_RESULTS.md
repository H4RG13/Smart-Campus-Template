# Load Test Results

Run via `scripts/load-test.js` (k6) against a fully containerized `api` + `postgres`
stack (`docker compose up -d postgres api`, then k6 in a container on the same
Docker network) — not against a mocked or in-memory backend. See
`docs/DEPLOYMENT_ARCHITECTURE.md` and `PLAN.md` Phase 6 for why this scale
(hundreds of users, tens of devices) is the target, not thousands.

## Scenario

- **staff**: ramps 0→30 concurrent virtual users over 15s, holds 30 for 30s, ramps
  down — simulating a school's staff logging in and checking student/attendance
  data over the course of a morning, not an instant all-at-once spike.
- **devices**: 20 constant virtual users hitting `/health` repeatedly (standing in
  for steady-state device traffic) for 50s.

## Command

```bash
docker compose -f deploy/docker-compose.yml up -d postgres api
docker run --rm -i --network deploy_default -e BASE_URL=http://api:8080 \
  -v "$(pwd)/scripts:/scripts:ro" grafana/k6 run /scripts/load-test.js
```

## Results (final run)

| Metric | Result |
|---|---|
| Total requests | 2,244 over ~51s (44/s average) |
| `http_req_duration` p95 | **23.57ms** (threshold: <500ms — passed) |
| `http_req_duration` p90 | 11.69ms |
| `http_req_duration` max | 765.56ms |
| Overall error rate | **0.44%** (threshold: <1% — passed) |
| `/health` | 100% success |
| `/students`, `/attendance-records` | 100% success (once authenticated) |
| `/auth/login` | 20/30 succeeded (66%) |

At this scale, the API's actual request handling is fast and reliable — p95 under
25ms is far inside the 500ms threshold, meaning there's substantial headroom above
"tens of devices, hundreds of users" before latency becomes a concern.

## Two real findings, not just numbers

**1. A load-testing artifact that mirrors a real risk.** The first test run used
k6's `constant-vus` executor, spiking all 30 staff VUs simultaneously at t=0. Combined
with a login-retry bug in the test script itself (a failed login wasn't cached,
so that VU retried login on *every* subsequent iteration), this created a
self-sustaining retry storm that kept the per-IP auth rate limiter permanently
saturated — 67% overall failure rate. Fixed by:
- Rewriting the scenario to ramp up gradually (`ramping-vus`), which is also more
  realistic — staff don't all log in the same second.
- Fixing the test script to attempt login exactly once per VU, matching how a real
  user behaves (they see an error and stop, they don't hammer retry).

This is worth internalizing beyond the test script: **a real client that retries
aggressively without backoff against a rate-limited endpoint can lock itself out
indefinitely**, since the fixed window never gets a chance to clear. The app's own
frontend doesn't do this (a human clicks "sign in" once), but it's a real
consideration for anything else that might call this API programmatically.

**2. The auth rate limit was tuned from an assumption, not a measurement — and
the load test corrected it.** The original limit (10 logins/min per IP) was set
assuming brute-force protection was the only concern. Running this test surfaced
that even 20-30 *legitimate* staff logging in within a short window from one
school's shared NAT'd IP can plausibly exceed a limit that low. Raised to 20/min
(see the comment in `Program.cs` next to the policy) — still low enough to bound a
real brute-force attempt, high enough that one person's login rush doesn't lock out
the rest of the building.

## Re-running this test

Re-run `scripts/load-test.js` after any change to authentication, rate limiting, or
core CRUD endpoints, and before every major version bump — a regression here is a
regression every school using this deployment would feel.
