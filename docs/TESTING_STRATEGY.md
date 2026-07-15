# Testing Strategy

Testing effort is allocated by *risk of silent harm*, not by chasing a coverage percentage (Rule #11). A bug in the Attendance Engine produces a wrong record a parent or auditor might never question; a bug in a DTO mapper produces a compile error or an obviously broken screen. The strategy weights accordingly.

## Test pyramid, mapped to layers

```
                    ▲
                   /E2E\              A handful — critical golden paths only
                  /------\
                 /  API   \           Integration tests: controller → real Postgres (Testcontainers)
                /----------\
               /Application \        Unit tests: use cases, validation rules — mocked ports
              /--------------\
             /     Domain     \      Unit tests: entities, invariants — no mocks needed at all
            /____________________\
```

## Domain layer — unit tests, heavy investment

**What:** entity invariants and value object rules — e.g. "an `AttendanceRecord` cannot have both `RecordedByUserId` and `DeviceEventId` set," "a `CalendarException` date must fall within its term's date range."

**Why heavy:** these are pure functions/objects with zero infrastructure dependency — cheapest tests to write and run, and they're the last line of defense if a use case forgets to check an invariant.

**Tooling:** xUnit, no mocking framework needed — Domain has nothing to mock.

## Application layer — unit tests, heaviest investment, especially Attendance/RBAC/RFID

**What:** use case logic — e.g. "given a scan at 08:05 with a 08:00 late-threshold and no calendar exception, the result is `Late`"; "a Teacher role cannot delete a Staff record"; "a duplicate `deviceEventId` is rejected without creating a second attendance record."

**Why heaviest:** this is where the business rules Rule #11 calls out by name live. A regression here is the one most likely to reach production data undetected (nothing crashes — it just silently records the wrong thing).

**Tooling:** xUnit + a mocking library (e.g. NSubstitute) for the Application-layer interfaces (ports) like `IEmailSender`, `IClock` — never mocking `DbContext` directly; Application tests exercise real Domain objects, only the outward-facing infrastructure ports are faked.

**Practice:** every new rule added to the Attendance Engine, RBAC policy, or RFID validation ships with a test in the same PR — enforced in code review per Rule #11, not by a coverage gate.

## Infrastructure/API layer — integration tests, moderate investment

**What:** does the EF Core mapping actually round-trip correctly against a real Postgres; does a controller wire auth/validation/use case together correctly end-to-end for the important flows (login, manual attendance entry, device event ingestion).

**Why moderate, not heavy:** these tests are slower and more brittle than Application unit tests (they touch a real database), so they're reserved for verifying the *wiring* is correct — the business-rule correctness is already covered at the Application layer.

**Tooling:** `Testcontainers` spinning up a real PostgreSQL container per test run (not SQLite-in-memory — SQLite's behavior diverges from Postgres enough, e.g. on `jsonb` or timestamp handling, that testing against it would give false confidence). `WebApplicationFactory` for in-process API hosting.

**Coverage target:** one integration test per module's primary CRUD path plus its one or two most important business flows (e.g. Attendance module: manual check-in, device-event ingestion, duplicate-event rejection) — not every endpoint permutation.

## Firmware — bench/manual testing, not automated CI

**What:** RFID read reliability, offline queue behavior under simulated network loss, LED/buzzer feedback correctness.

**Why not automated:** firmware testing against real hardware (ESP32 + MFRC522) isn't practically automatable in CI without a hardware test rig, which isn't justified at this project's scale. Instead: a documented manual bench-test checklist (tag read, simulated Wi-Fi drop + reconnect, queue drain, heartbeat) run before flashing devices for a new school deployment (tied to `PLAN.md` Phase 4/6 exit criteria).

**What is testable in CI:** any pure logic that can be factored out of the firmware's hardware-interaction code (e.g. a retry-backoff calculation) — extracted and unit tested if and when such logic exists; not forced.

## Frontend — light integration + no exhaustive unit testing of components

**What:** critical user flows via component/integration tests (React Testing Library) — login, viewing a student's attendance, marking manual attendance, viewing device status. Not every presentational component in isolation.

**Why light:** UI composed of simple, mostly-presentational components backed by TanStack Query has a low bug surface relative to backend business logic; effort is better spent on the backend layers where an undetected bug has real consequence.

**E2E:** a small Playwright suite (not exhaustive) covering the true golden paths per role (Admin creates a student → assigns RFID tag → attendance shows up on dashboard) — run before a release tag, not on every commit, to keep CI fast (see `CICD.md`).

## What's explicitly out of scope for V1

- No mutation testing, no property-based testing — valuable but disproportionate tooling investment for this team size/project stage.
- No load/performance test suite in CI — a manual load test pass happens once per `PLAN.md` Phase 6 (pre-first-deployment hardening), not on every commit, since single-school scale rarely regresses meaningfully between releases.
- No visual regression testing — theming varies deliberately per school (branding), which would make visual-diff baselines expensive to maintain for little benefit at this scale.
