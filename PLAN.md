# SmartCampus Template — Build Plan

This is the phased plan for building the template itself (Phase 1 of the broader product lifecycle described in `README.md`). It assumes the architecture in `docs/ARCHITECTURE.md` and the guardrails in `RULES.md` as given.

Each phase should end with something runnable end-to-end, even if narrow — never a phase that's "half a layer" with nothing working.

---

## Phase 0 — Scaffold

**Goal:** empty-but-correct solution structure that reflects the layering decisions, before any real feature exists.

- [ ] Create solution + 5 backend projects (`Domain`, `Application`, `Infrastructure`, `Api`, `Configuration`) with correct project references (dependency rule enforced: Domain has zero references)
- [ ] Wire `Program.cs` with DI registration stubs, Serilog, Swagger, health check endpoint
- [ ] Add `AppDbContext` with EF Core + PostgreSQL provider, empty initial migration
- [ ] Scaffold React app (Vite + TS + Tailwind + React Router + TanStack Query + Zustand) with a placeholder route and a `/config` fetch wired to a stub backend endpoint
- [ ] `docker-compose.yml` bringing up API + Postgres + client dev server
- [ ] CI: GitHub Actions workflow that builds backend + frontend and runs (empty) test suites on every PR

**Exit criteria:** `docker compose up` serves a blank React app that successfully calls a real backend `/health` and `/config` endpoint against a real Postgres container.

---

## Phase 1 — Identity, RBAC, Configuration

**Goal:** the two things every other feature depends on: who is logged in, and what does this school look like.

- [x] `SchoolSettings` / `BrandingSettings` / `IotSettings` strongly-typed config models, bound from environment variables, validated at startup (fail fast on missing values)
- [x] User entity, roles (Admin, Teacher, Staff, Parent, Student), password hashing
- [x] JWT issuance + validation middleware
- [x] Role-based authorization policies wired into the API
- [x] `/config` endpoint returning branding (name, logo URL, theme colors, timezone) for frontend consumption
- [x] Frontend: login flow, auth state in Zustand, protected routing, theme injected from `/config`

**Exit criteria:** a seeded admin user can log in, see the school's branding rendered from config (not hardcoded), and reach a role-gated page.

---

## Phase 2 — Students, Staff, Academic Calendar

**Goal:** the data model everything else (attendance, notifications) hangs off of.

- [x] Student and Staff entities, CRUD use cases (Application feature slices), FluentValidation rules
- [x] Class/Section structure, enrollment
- [x] Academic Calendar (terms, holidays, exceptions)
- [x] Frontend: student/staff list + detail views, forms via React Hook Form

**Exit criteria:** an admin can create a term, enroll a student into a class, and see it reflected in the UI without touching code.

---

## Phase 3 — Attendance Engine

**Goal:** the core business-rule module — this is where lateness thresholds, absence policies, and manual overrides live.

- [x] Attendance entity + rules (on-time/late/absent thresholds, configurable per school via `SchoolSettings`)
- [x] Manual check-in/override use cases + RBAC (teacher/admin only)
- [x] Academic Calendar integration (no attendance expected on holidays)
- [x] Audit trail hook (who marked/changed an attendance record)
- [x] Frontend: attendance dashboard, manual entry UI, ECharts summary (daily/weekly attendance rate)

**Exit criteria:** attendance can be recorded manually end-to-end, respecting calendar and lateness config, with a visible audit trail.

---

## Phase 4 — RFID / IoT Engine

**Goal:** ESP32 devices can authenticate, submit attendance events, and report health — with all business logic staying server-side.

- [x] Device registration + per-device auth token issuance
- [x] Ingestion endpoint: validate device identity, dedupe by device event ID (idempotency), apply Attendance Engine rules (not duplicated in firmware)
- [x] Heartbeat/health endpoint + basic device status view
- [x] Firmware (`firmware/esp32-rfid/`): RFID read, device auth, offline queue + retry, heartbeat, LED/buzzer feedback, firmware version reporting — **written to match the protocol but not bench-tested on real hardware; see the firmware's own README for known gaps**
- [x] Frontend: device status dashboard (online/offline, last heartbeat)

**Exit criteria:** a physical (or bench-test) ESP32 scanning a tag produces a real attendance record through the same Attendance Engine used by manual entry, survives a simulated network drop (offline queue + retry), and shows up on the device dashboard. **Verified via curl simulation of the device protocol** (register → scan → attendance record created with the same `AttendanceStatusCalculator` as manual entry, duplicate `deviceEventId` rejected, unknown tag rejected, device shows online/offline correctly) — the actual firmware has not been run against physical hardware.

---

## Phase 5 — Notifications & Reporting

**Goal:** close the loop to parents/staff and produce the reports schools actually ask for.

- [x] Notification channel abstraction (SMTP first), triggered on absence per school-configurable rules
- [x] Standard attendance reports (daily/weekly/monthly, exportable)
- [x] Clearly isolated `Reports/Custom/` area for the first school's bespoke report needs (proves out the extension pattern before it's needed twice)

**Exit criteria:** a parent receives an absence email, and an admin can export a standard attendance report. **Verified for real** — spun up a MailHog SMTP catcher, marked a student absent, and confirmed the email actually arrived (not just a mocked/logged send); CSV export confirmed via the frontend proxy with the correct `Content-Disposition` header. Added Guardian/GuardianStudent (deferred from Phase 2) since notifications need someone to notify.

---

## Phase 6 — Hardening for First Deployment

**Goal:** everything needed to actually hand this to School #1, not just run it in dev.

- [ ] Rate limiting on public/device-facing endpoints
- [ ] Secrets management pass (no secrets in repo, `.env` + Docker secrets documented)
- [ ] NGINX + Let's Encrypt SSL in `deploy/`
- [ ] Rename/clone checklist scripted and tested against a throwaway clone
- [ ] Backup/restore procedure for the Postgres volume documented
- [ ] Load test at realistic single-school scale (hundreds–low thousands of users, tens of devices)
- [ ] Testing Strategy, Monitoring, Logging, CI/CD deliverables written up (see remaining deliverables list)

**Exit criteria:** a fresh clone can go from "requirement gathering" to "deployed and training-ready" following only the documented checklist, with no undocumented manual steps.

---

## Ordering rationale

Identity/Config before Students/Staff before Attendance before IoT before Notifications — each phase's data or rules are a hard dependency of the next. IoT is deliberately Phase 4, not Phase 1, despite being a headline feature: attendance business rules must exist and be provably correct against manual entry *before* a physical device is allowed to write to the same table. This avoids debugging "is this a firmware bug or a business rule bug" simultaneously.

## Explicitly out of scope for V1

- `SmartCampus.Core` extraction (see `docs/ARCHITECTURE.md` §9) — revisit after 2–3 real deployments
- Redis, multi-region, or any scaling infrastructure beyond a single-school VPS
- QR/NFC support (firmware extension point only, not built)
- Parent/student self-service portals beyond basic notification receipt (candidate for a later phase, pending client demand)
