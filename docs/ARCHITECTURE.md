# SmartCampus Template — Architecture

## 1. System Vision

SmartCampus Template is a **reusable, white-labelable school management system** that our agency clones per client engagement. It is not a product schools sign up for — it's a codebase we hand-tailor and deploy once per school, then maintain independently for years.

The vision is narrow on purpose: solve the operational core every school shares (people, attendance, academic structure, notifications, RFID-based access/attendance) extremely well, and leave everything school-specific (branding, unique policies, custom reports) as configuration or thin per-client extensions — never as forks of core logic.

Success looks like: a new developer can clone the template, follow a checklist, and have a school live in days, not months — and five years from now, a junior developer can still open the codebase and understand it without a whiteboard session.

## 2. Product Philosophy

- **Boring is good.** Every pattern used must earn its place by solving a concrete, present problem — not a hypothetical future one.
- **Optimize for the next developer, not the current one.**
- **Configuration over forking.** The measure of success for "per-school customization" is: did we change a config value, or did we change code?
- **Extraction later, not abstraction now.** `SmartCampus.Core` gets extracted once real reuse is observed across 2–3 schools, not guessed at from day one.
- **A modular monolith, not a distributed system.** One well-organized ASP.NET Core application per school is dramatically cheaper to build, deploy, debug, and hand off than a distributed system — and matches what a small agency can staff and support.

## 3. High-Level Architecture

Modular Monolith, organized as Clean Architecture layers, with feature-based (vertical slice) organization inside the Application layer.

```
┌─────────────────────────────────────────────────┐
│                  Presentation                    │
│   ASP.NET Core Web API (Controllers/Endpoints)   │
│   SignalR Hubs · Swagger · Middleware            │
└───────────────────────┬───────────────────────────┘
                         │ depends on
┌───────────────────────▼───────────────────────────┐
│                    Application                     │
│  Feature Slices (Attendance, Students, IoT, etc.)  │
│  Use Cases / Services · DTOs · Validation           │
│  Interfaces for Infrastructure (ports)              │
└───────────────────────┬───────────────────────────┘
                         │ depends on
┌───────────────────────▼───────────────────────────┐
│                       Domain                        │
│  Entities · Value Objects · Domain Rules            │
│  No dependencies on anything else                   │
└─────────────────────────────────────────────────────┘
                         ▲
                         │ implements interfaces from Application
┌───────────────────────┴───────────────────────────┐
│                  Infrastructure                     │
│  EF Core + PostgreSQL · Redis · MinIO · SMTP        │
│  IoT Ingestion (ESP32) · JWT                        │
└─────────────────────────────────────────────────────┘
```

**Why Clean Architecture (pragmatically applied):** the dependency rule (inner layers know nothing about outer layers) is the one piece of enterprise discipline worth keeping — it lets us swap Postgres or add a second delivery mechanism without rewriting business rules. CQRS, mediators, and event sourcing are deliberately **not** adopted unless a specific module demonstrably needs them (see §8).

**Why feature-based/vertical slices inside Application:** a developer working on "Attendance" should open one folder and see everything about attendance, not hunt across a layer-first `Services/`/`Repositories/`/`Validators/` split. Layer-first organization scales poorly past ~10 features; feature-first scales fine to 50+.

## 4. Folder Structure

```
SmartCampus.Template/
├── src/
│   ├── SmartCampus.Api/                    # Presentation
│   │   ├── Controllers/
│   │   ├── Hubs/                           # SignalR
│   │   ├── Middleware/
│   │   ├── Filters/
│   │   └── Program.cs
│   │
│   ├── SmartCampus.Application/            # Application (feature slices)
│   │   ├── Common/
│   │   │   ├── Interfaces/                 # Ports: IEmailSender, IFileStorage, etc.
│   │   │   ├── Behaviors/                  # Validation pipeline, logging pipeline
│   │   │   └── Exceptions/
│   │   ├── Features/
│   │   │   ├── Students/
│   │   │   ├── Attendance/
│   │   │   ├── Rfid/
│   │   │   ├── Staff/
│   │   │   ├── AcademicCalendar/
│   │   │   ├── Notifications/
│   │   │   └── Identity/
│   │   └── DependencyInjection.cs
│   │
│   ├── SmartCampus.Domain/                 # Domain
│   │   ├── Entities/
│   │   ├── Enums/
│   │   ├── ValueObjects/
│   │   └── Exceptions/
│   │
│   ├── SmartCampus.Infrastructure/         # Infrastructure
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/             # EF entity configs
│   │   │   └── Migrations/
│   │   ├── Identity/                       # JWT, password hashing
│   │   ├── Storage/                        # MinIO abstraction
│   │   ├── Messaging/                      # SMTP, SignalR broadcast
│   │   ├── Iot/                            # ESP32 ingestion, device auth
│   │   └── DependencyInjection.cs
│   │
│   └── SmartCampus.Configuration/          # Cross-cutting config models
│       ├── SchoolSettings.cs
│       ├── BrandingSettings.cs
│       └── IotSettings.cs
│
├── tests/
│   ├── SmartCampus.Domain.Tests/
│   ├── SmartCampus.Application.Tests/
│   └── SmartCampus.Api.IntegrationTests/
│
├── firmware/
│   └── esp32-rfid/                         # PlatformIO/Arduino project
│
├── client/                                 # React frontend
│   ├── src/
│   │   ├── features/                       # mirrors backend feature slices
│   │   ├── shared/
│   │   ├── app/                            # routing, providers, store
│   │   └── config/                         # branding/theme injection
│   └── vite.config.ts
│
├── deploy/
│   ├── docker-compose.yml
│   ├── nginx/
│   └── .env.example
│
└── docs/
```

**Rationale:** the `Features/` folder under Application is the load-bearing decision — it's what makes future extraction to `SmartCampus.Core` tractable (§9), because reusable vs. school-specific logic is already segregated by folder rather than tangled together.

## 5. Project Responsibilities

| Project | Responsibility | Must NOT contain |
|---|---|---|
| **SmartCampus.Domain** | Entities, value objects, invariants true regardless of database/UI/framework (e.g., "an attendance record cannot predate enrollment") | EF Core attributes, HTTP concerns, any framework reference |
| **SmartCampus.Application** | Use-case orchestration, DTOs, FluentValidation rules, interfaces (ports) that Infrastructure implements | Direct EF Core queries, direct HTTP/SignalR code |
| **SmartCampus.Infrastructure** | EF Core implementation, Postgres, Redis, MinIO, SMTP, JWT token generation, ESP32 device ingestion logic | Business rules (e.g., lateness thresholds belong in Application/Domain, not the ingestion handler) |
| **SmartCampus.Api** | HTTP routing, auth middleware, request/response shaping, SignalR hub wiring, Swagger | Business logic — controllers stay thin, delegating to Application |
| **SmartCampus.Configuration** | Strongly-typed config models bound from environment variables/appsettings, per-school | Hardcoded per-school values |
| **client/ (React)** | UI rendering, client-side state (Zustand), API consumption (TanStack Query) | Business rule enforcement beyond basic UX validation (server is source of truth) |
| **firmware/** | Read RFID, authenticate device, queue/retry, report heartbeat, LED/buzzer feedback | Any decision logic (e.g., "is this student late") — that's a backend concern |

## 6. Technology Stack

- **ASP.NET Core (.NET 9) + EF Core + PostgreSQL** — mature, strongly-typed, excellent tooling for a small team; Postgres is free, robust, and trivially self-hostable per school.
- **Redis (optional)** — wired as an abstraction (`ICacheProvider`) but not forced on every deployment; enabled only when a specific school's load warrants it.
- **JWT Authentication + RBAC** — stateless auth fits per-school independent deployment cleanly; no shared session store needed.
- **SignalR** — used narrowly, for real-time attendance/device status dashboards, not as a general-purpose event bus.
- **Serilog** — structured logging to file/console per container; no external log aggregation forced by default.
- **React + TypeScript + Vite + Tailwind + TanStack Query + React Hook Form + Zustand + ECharts** — standard, hire-able stack. Zustand specifically because Redux-level ceremony isn't justified here.
- **Docker + Docker Compose + NGINX** — matches "independent deployment per school" exactly: one compose stack, one NGINX reverse proxy with SSL, one VPS, no orchestration platform needed.
- **MinIO abstraction** — behind an `IFileStorage` interface so a school can point at local disk, MinIO, or S3-compatible storage without touching Application code.

**Explicitly rejected:** Kubernetes, RabbitMQ/Kafka, microservices, multi-tenant shared DB — all violate the independent-per-school, small-agency-maintainable mandate.

## 7. Module Breakdown

| Module | Responsibility | Notes |
|---|---|---|
| **Identity & RBAC** | Login, roles (Admin, Teacher, Staff, Parent, Student), permission checks | Candidate for `SmartCampus.Core` once patterns stabilize across 2–3 schools |
| **Students** | Enrollment, profile, class/section assignment | School-specific fields via config-driven extra fields, not schema forks |
| **Staff** | Employee records, roles, assignments | |
| **Attendance Engine** | On-time/late thresholds, absence policies, manual override, RFID-triggered check-in | Business logic lives here, NOT in firmware or ingestion layer |
| **RFID/IoT Engine** | Device registration, device auth tokens, event ingestion, heartbeat monitoring, offline queue reconciliation | Validates device identity before trusting events |
| **Academic Calendar** | Terms, holidays, exceptions that attendance rules consult | |
| **Notifications** | Email/SMS to parents on absence, configurable per school | Pluggable channel interface |
| **Branding/Configuration** | School name, logo, theme colors, timezone — loaded at startup, injected into frontend via `/config` | Never compiled into code |
| **Audit Trail** | Who changed what, when — implemented as a lightweight EF interceptor | Not a bespoke event-sourcing system |
| **Reporting** | Attendance reports, exports | Per-school custom reports isolated in `Reports/Custom/`, never touching core modules |

## 8. Architectural Decisions

**Modular Monolith over Microservices** — one deployable unit per school matches the business model exactly; microservices solve organizational/scaling problems this project doesn't have. Trade-off (less independent sub-component scaling) accepted deliberately — no school approaches the load where it matters.

**No CQRS/Mediator by default** — adds indirection that pays off mainly at larger team/complexity scale. A small team can call a use-case class directly. If a generic cross-cutting need arises (e.g. validation pipeline), add a lightweight pipeline behavior without adopting full mediator ceremony everywhere.

**No Domain Events / Event Sourcing** — attendance and enrollment histories are naturally modeled as timestamped rows with an audit trail, not event streams needing replay.

**Repository Pattern used selectively, not universally** — EF Core's `DbContext` already is a unit-of-work/repository in practice. Add explicit repository interfaces only where Application genuinely needs decoupling from EF Core specifics (e.g. complex reused query composition), not as a blanket rule.

**Configuration-first branding/behavior** — the single most important decision enabling the clone-and-deploy business model. Business rules that vary must be configuration, never `if (schoolName == "X")` branches. Violating this means every deployment silently forks logic and "the template" becomes fiction after 3 clients.

**ESP32 firmware is dumb; backend is smart** — firmware update cycles are slow and hard to roll back across many physical devices; backend logic can be fixed and redeployed same-day. Any rule (lateness, duplicate scan debounce, permissions) belongs server-side.

## 9. Future SmartCampus.Core Extraction Strategy

V1 does not create `SmartCampus.Core`. V1 creates the seams that make extracting it later mechanical instead of a risky rewrite.

- **Segregation by folder, not by project.** Inside `Application/Features/`, school-agnostic code (attendance lateness algorithm, RBAC permission checks, RFID device-auth handshake) depends only on Domain and abstract interfaces — never on a school-specific config value baked directly into logic. Inherently school-specific code (custom report formatting, one school's unusual approval workflow) stays in clearly named `Custom/` subfolders.
- **No premature `SmartCampus.Core.csproj`.** Designing its public API from a sample size of one is almost always wrong and expensive to unwind.
- **Trigger for extraction:** after 2–3 real school deployments, audit which modules turned out near-identical across schools. Candidates: Authentication, Attendance Engine core algorithm, RFID Engine, Logging setup, Validation base classes, Permission Engine, Audit Trail, common DTOs/Extensions/Exceptions, Middleware.
- **Extraction mechanics:** turn `SmartCampus.Core` into a versioned, privately-hosted NuGet package; each school's layers reference the package instead of local files. Because Application already depends only on interfaces, this is a reference swap, not a redesign.
- **Permanent disqualification from Core:** anything with a school's name, logo, theme, connection string, or bespoke policy in it.

## 10. Development Workflow

Per-client rollout, architecturally supported as follows:

1. **Create Repository from Template** — via "Use this template," never manual copy-paste.
2. **Rename Project** — a single documented checklist/script (namespace, solution name, container names) — a 10-minute mechanical task.
3. **Configure Branding & Environment Variables** — populate `.env` and `SchoolSettings`/`BrandingSettings`; no code changes.
4. **Provision PostgreSQL** — one database per school, run versioned EF Core migrations.
5. **Configure IoT Devices** — register device IDs/keys; flash firmware with that school's server endpoint and credentials.
6. **Deploy** — `docker compose up` against the school's VPS, NGINX + SSL.
7. **Training & Maintenance** — standard agency process, independent per client.

## 11. Git Strategy

- **`SmartCampus-Template` is the source of truth** for shared/core improvements.
- **Each school repo is a git-detached clone**, not a fork-with-upstream-tracking — intentional, so schools never accidentally receive another client's changes.
- **Propagating a fix to existing school repos:** add the template repo as a `git remote` named `template` in each school repo; `git fetch template && git cherry-pick <fix-commit>` as a deliberate, reviewed action per school — never automatic.
- **Conventional commits** (`feat:`, `fix:`, `chore:`) so cherry-picking and changelogs stay tractable across repos.
- **Tags on the template repo** (`v1.0.0`, `v1.1.0`) mark stable baselines so we always know which template version a school was cloned from.

## 12. Branching Strategy

- **`main`** — always deployable.
- **`develop`** (optional, template repo only) — integration branch for unreleased template features; school repos can usually skip this and branch straight off `main`.
- **`feature/<name>`** — short-lived, merged via PR.
- **`hotfix/<name>`** — urgent production fix on a school repo, branched from `main`, merged back and tagged.
- No GitFlow ceremony beyond this — one deployment target per repo makes the multi-environment machinery GitFlow was designed for unnecessary.

## 13. Coding Philosophy

- Readability over cleverness.
- Explicit over implicit.
- YAGNI as a default stance — build the extension point after two concrete cases need it, not one imagined case.
- Consistency over local optimization.
- Tests protect business rules, not a coverage number — prioritize Attendance Engine, RBAC, RFID validation (Domain/Application); lighter touch on API integration tests; don't chase 100% on DTOs/plumbing.
- Comments explain "why," never "what."

## 14. Technical Risks

| Risk | Impact | Mitigation |
|---|---|---|
| Template drift across school repos | High | Strict folder segregation (§9); extend via config/isolated override, not in-place edits to generic modules |
| Config sprawl | Medium | Strongly-typed config POCOs validated at startup, not a loose key-value bag |
| IoT device trust/security | Medium-High | Per-device auth tokens, server-side validation of identity/plausibility, rate limiting on ingestion endpoints |
| Per-school maintenance burden growing linearly with client count | Medium (inherent) | Mitigated, not eliminated, by `SmartCampus.Core` extraction once patterns stabilize |
| Junior developers reaching for unneeded patterns | Medium | This document + `RULES.md` codify the rationale so it's discoverable, not tribal knowledge |
| Offline ESP32 devices losing attendance events | Medium | Firmware offline queue + retry, idempotent server-side ingestion (dedupe by device event ID) |
| Secrets management across many independent VPS deployments | Medium | Standardized `.env` + Docker secrets per school, never committed, documented rotation procedure |

## 15. Recommendations

1. Adopt this architecture as-is for V1 — it matches the business model's actual constraints, not aspirational scale.
2. Do not create `SmartCampus.Core` until after the 2nd or 3rd school deployment.
3. Invest early in the rename/clone checklist and deploy scripts — highest-leverage automation for this business model.
4. Keep `RULES.md` current as the enforceable version of §8, §13, §14.
5. Keep Redis, message queues, and any "just in case" infrastructure out of the default `docker-compose.yml`.
6. Revisit this document after the first production deployment — update it with what was actually learned deploying to School #1 before School #2 begins.
