# SmartCampus Template — Engineering Rules

These are non-negotiable guardrails, not suggestions. They exist so the architectural discipline in [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) survives contact with deadlines, new hires, and time. If a rule here seems to block a real need, raise it for discussion and update this file — don't silently work around it.

Every rule states **what**, **why**, and **how to check it**.

---

## 1. Dependency direction is absolute

**Rule:** `Domain` references nothing. `Application` references only `Domain`. `Infrastructure` and `Api` may reference `Application` and `Domain`, never the other way around.

**Why:** this is what lets us change databases, add a second delivery mechanism, or unit-test business rules without spinning up EF Core or ASP.NET Core. Break it once and every "simple" future change requires touching layers that shouldn't know about each other.

**How to check:** if you're adding a `using` for `Microsoft.EntityFrameworkCore`, `Microsoft.AspNetCore.*`, or `Npgsql` inside `SmartCampus.Domain` or `SmartCampus.Application`, stop — you're in the wrong layer.

---

## 2. No school-identity branching in code

**Rule:** Never write `if (schoolName == "X")`, `if (tenantId == ...)`, or any conditional keyed on a specific school's identity, inside application logic.

**Why:** this is the rule that keeps the template a template. The first such branch is harmless; the tenth means the codebase has silently forked per client and "shared logic" is a fiction.

**How to check:** if a behavior needs to differ per school, it must be represented as a value in `SchoolSettings` / `BrandingSettings` / `IotSettings` (or a new strongly-typed config class), read at runtime — not as a code branch, not as a compiler flag, not as a school-named subclass.

---

## 3. Configuration is strongly typed, not a key-value bag

**Rule:** All per-school configuration is bound to a POCO (e.g. `SchoolSettings`, `BrandingSettings`, `IotSettings`) via the Options pattern, validated at startup. No raw `IConfiguration["SomeKey"]` lookups scattered through feature code.

**Why:** loose string-keyed config sprawls invisibly and fails silently (typo a key, get `null`, find out in production). A typed, validated model fails fast at startup instead.

**How to check:** new config values get a new property on an existing settings class, or a new settings class registered in `DependencyInjection.cs` — never a bare `Configuration["X"]` call in a controller or use case.

---

## 4. Business logic never lives in firmware

**Rule:** ESP32 firmware reads RFID tags, authenticates itself, queues/retries offline, reports heartbeat and firmware version, and gives LED/buzzer feedback. It does not decide whether a scan counts as "on time," "late," or "duplicate." Every such decision is made server-side by the Attendance Engine, and the firmware just displays the server's response.

**Why:** firmware deployed to physical devices across a school (or across schools) is slow and risky to update. A bug in a business rule must be fixable same-day on the backend, not require re-flashing hardware.

**How to check:** if you're writing a threshold, a comparison against a business rule, or a decision in the `firmware/` folder, move it to `SmartCampus.Application/Features/Attendance` or `Rfid` instead.

---

## 5. IoT ingestion is never trusted blindly

**Rule:** every device-originated event must be authenticated (per-device token/key) and validated for plausibility (timestamp sanity, duplicate/dedup by device event ID) before it's accepted as an attendance record.

**Why:** ESP32 devices are physically accessible; a compromised or misbehaving device must not be able to write fraudulent or duplicate attendance data.

**How to check:** any new ingestion endpoint must go through the existing device-auth middleware and idempotency check — don't add a new unauthenticated or unvalidated path "just for now."

---

## 6. No CQRS, mediator, domain events, or event sourcing by default

**Rule:** use cases are plain classes/methods called directly from controllers via DI. Don't introduce MediatR, a generic event bus, domain events, or event sourcing without a specific, articulated problem these solve that a direct method call doesn't.

**Why:** these patterns pay off at a scale and team size this project doesn't have. Introducing them "because it's best practice" adds indirection every future reader has to pay for, with no corresponding benefit here.

**How to check:** if a PR introduces one of these patterns, the PR description must name the concrete problem being solved and why a direct approach doesn't work. Absent that, the PR should be simplified.

---

## 7. Repository pattern is opt-in, not default

**Rule:** query and persist directly against `AppDbContext` (EF Core) in Infrastructure unless a feature has a genuine, stated need to decouple Application from EF Core specifics (e.g. a complex query reused across multiple features).

**Why:** `DbContext` already functions as a unit-of-work/repository. A blanket `IStudentRepository`-per-entity layer usually just forwards to EF Core with no real swap target, adding a layer of indirection for no benefit.

**How to check:** before adding a new repository interface, confirm there's a concrete reuse or decoupling need — not just "it's the pattern."

---

## 8. No new infrastructure without an observed need

**Rule:** don't add Redis, a message queue, Kubernetes, or any additional infrastructure to a school's `docker-compose.yml` speculatively. Add it only when a specific, observed load or operational problem at that school justifies the cost.

**Why:** every school is deployed independently to a single VPS; extra infrastructure means extra operational burden multiplied across every client, for a problem that likely doesn't exist yet.

**How to check:** a PR adding new infrastructure must cite the specific observed problem (load test result, production incident, concrete requirement) it addresses.

---

## 9. `SmartCampus.Core` does not exist yet — don't create it prematurely

**Rule:** do not create a `SmartCampus.Core` shared library/package until after at least 2–3 real school deployments have proven which modules are genuinely reusable unchanged.

**Why:** a shared library designed from one school's code is a guess, not an extraction, and is expensive to redesign once other schools depend on it.

**How to check:** in the meantime, keep school-agnostic code physically segregated in `Application/Features/*` (depending only on `Domain` and interfaces) from school-specific code in clearly named `Custom/` subfolders, so extraction later is a folder move, not a rewrite.

---

## 10. Comments explain "why," never "what"

**Rule:** don't write a comment describing what a line of code does if a well-named identifier already makes that obvious. Only comment a non-obvious constraint, workaround, or invariant.

**Why:** redundant comments rot — they drift from the code and add noise without adding understanding. A comment should carry information the code can't.

**How to check:** if deleting a comment wouldn't confuse a future reader, delete it.

---

## 11. Tests protect business rules, not a coverage number

**Rule:** prioritize unit tests on `Domain` and `Application` logic for Attendance Engine rules, RBAC/permission checks, and RFID event validation. Integration-test the API surface at a lighter touch. Don't chase 100% coverage on DTOs, mappers, or other plumbing.

**Why:** the Attendance Engine and RBAC are where a silent bug causes real harm (wrong attendance record, wrong access). Coverage on a DTO's getter/setter protects nothing.

**How to check:** a PR touching Attendance/RBAC/RFID logic without an accompanying test should be questioned in review.

---

## 12. Git: template fixes propagate deliberately, never automatically

**Rule:** shared/core fixes developed against `SmartCampus-Template` reach a deployed school repo only via an explicit `git fetch template && git cherry-pick <commit>` (or reviewed merge), performed per school, never via an automated sync job.

**Why:** each school repo may have diverged; blind automatic propagation risks silently overwriting a school's customization.

**How to check:** if you find yourself wanting to script "push template updates to all school repos automatically," stop — that violates the independent-deployment model this project is built on.

---

## 13. One branch per feature/phase; reuse it for follow-up work on the same slice

**Rule:** when starting a new feature or a new `PLAN.md` phase, branch off `develop` (e.g. `feature/attendance-engine`, `phase-2-students-staff`). While that feature/phase is still in progress or under active review, push follow-up commits and fixes to the *same* branch rather than opening a new one for every small change. Only cut a new branch when starting genuinely new, separable work.

**Why:** a fresh branch per typo fix or review comment creates PR sprawl that's harder to review and harder to trace back to "which phase was this part of." Reusing the branch for its whole lifecycle keeps history readable and keeps CI/review effort focused on one moving target at a time.

**How to check:** before running `git checkout -b`, ask whether this is a continuation of work already in flight on an existing branch (reuse it) or the start of a new feature/phase (branch it). When in doubt, check `git branch -a` and the open PRs first.

---

## Amending this file

These rules encode real trade-offs, each with a stated reason. If a rule is blocking legitimate work, don't bypass it silently — propose the change, update the rule and its rationale here, and make sure `docs/ARCHITECTURE.md` §8/§13/§14 stay consistent with it.
