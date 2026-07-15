# Future Modules

Deliberately not built in V1. Listed here so the intent is documented and future work doesn't get bolted on in a way that violates the architecture — each entry states the trigger condition for building it, not a commitment to build it.

## SmartCampus.Core extraction

Already covered in depth in `ARCHITECTURE.md` §9 and `RULES.md` #9 — the single most important piece of "future work," restated here for completeness: extract after 2–3 real school deployments prove which modules are genuinely reusable unchanged (Authentication, Attendance Engine core, RFID Engine, Logging setup, Validation base classes, Permission Engine, Audit Trail, common DTOs/Extensions/Exceptions, Middleware).

## QR code / NFC scanning

**Trigger:** a client requests it, or RFID hardware costs/availability become a blocker for a specific deployment.

**Design implication:** the Attendance Engine already treats "a tag was scanned" as an abstract input (`rfidTagId` string), independent of the physical read mechanism — a QR/NFC scan is just a different device type producing the same `DeviceEvents` shape with a different tag format. Should require a new device type + firmware/scanning-app variant, not changes to the Attendance Engine itself. This is the RFID/IoT Engine module's design already accounting for it (Section 7 of `ARCHITECTURE.md`), not a structural change.

## Parent/student self-service portal

**Trigger:** client demand for parents to view attendance, receive richer notifications, or submit absence excuses themselves, beyond the current one-way notification (`NOTIFICATION_LOGS`).

**Design implication:** `GUARDIANS` and `GUARDIAN_STUDENT` already exist in the schema (`DATABASE_DESIGN.md`) precisely so this doesn't require a data model rework — it requires a new `features/parent-portal` frontend area and a scoped set of read (and eventually limited write, e.g. absence excuse submission) API endpoints with guardian-specific RBAC policies. Should not require changes to the Attendance Engine's core rules.

## Multi-campus support (single school, multiple physical locations)

**Trigger:** a specific client operates more than one campus under one administrative umbrella and wants one deployment covering both — different from multi-tenancy (still one client, one contract, one deployment), so it does not violate the "no multi-tenant shared runtime" principle.

**Design implication:** would introduce a `Campuses` table and a `CampusId` FK on `Classes`/`Devices` (not on every table — only where "which physical location" is meaningful), plus campus-scoped RBAC. This is additive to the existing schema, not a redesign — deferred until a real client needs it rather than speculatively adding a `CampusId` everywhere now (Rule #2's spirit: don't model a distinction that doesn't exist yet).

## Billing/payments module (tuition, fees)

**Trigger:** a client wants tuition/fee tracking or online payment collection integrated rather than handled by a separate system they already use.

**Design implication:** a new, clearly bounded feature slice (`Features/Billing`) with its own entities (`Invoices`, `Payments`) and a payment-gateway abstraction (`IPaymentProcessor`) analogous to `IFileStorage`/`IEmailSender` — deliberately kept out of the Attendance/Student core so a school that doesn't need it never has to configure or think about it. High caution warranted here: PCI-DSS scope and financial-data handling are a materially different risk profile than the rest of this system, and should get its own security review before being offered to any client, not be treated as "just another module."

## Advanced reporting / analytics (beyond `Reports/Custom`)

**Trigger:** recurring client requests for cross-cutting analytics (trend analysis, predictive absence risk) that outgrow the per-school `Reports/Custom/{reportKey}` extension point.

**Design implication:** if this becomes common across multiple schools, it's itself a `SmartCampus.Core` extraction candidate (a generic reporting/analytics engine) rather than something built bespoke per school indefinitely — a good illustration of the extraction trigger in practice: repeated, similar-shaped custom work across clients is the signal, not a upfront guess.

## Mobile app (native, beyond responsive web)

**Trigger:** client or field-usage demand (e.g. staff wanting a dedicated mobile check-in app rather than a mobile browser).

**Design implication:** the existing REST API (`API_SPECIFICATION.md`) already serves this without backend change — a native app is purely a new API consumer. No architectural prerequisite beyond what already exists; this is listed mainly to note that it's *not* blocked on anything, should a client want to fund it.

## Explicit non-goals (not "future," just out)

- **Multi-tenant SaaS conversion** — fundamentally contradicts the business model in `README.md`; would require a full redesign (shared DB with tenant isolation, shared runtime, shared risk surface) that this architecture is deliberately not built toward.
- **Kubernetes/microservices migration** — no scale trigger exists at single-school deployment sizes that would justify this; not a "later" item, a rejected direction (see `ARCHITECTURE.md` §6, §8).
