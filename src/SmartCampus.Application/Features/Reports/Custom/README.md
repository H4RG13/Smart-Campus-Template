# Custom Reports

This folder is the isolated extension point for a school's bespoke report needs —
see `docs/ARCHITECTURE.md` §9 and `RULES.md` #9. V1 ships with **zero** handlers
registered; `GET /reports/custom/{reportKey}` always 404s until a school actually
asks for something the standard `GetAttendanceReportUseCase` doesn't cover.

## Adding a custom report for a client

1. Implement `ICustomReportHandler` in a new file in this folder (e.g.
   `SpringFestivalAttendanceReportHandler.cs`). Give it a distinct `ReportKey`.
2. Register it in `DependencyInjection.cs`:
   `services.AddScoped<ICustomReportHandler, SpringFestivalAttendanceReportHandler>();`
3. It's now reachable at `GET /reports/custom/spring-festival-attendance` (or
   whatever `ReportKey` you chose) with no controller or routing changes needed —
   `ReportsController` already dispatches by key via `GetCustomReportUseCase`.

## Why this exists as a template pattern, not a generic reporting engine

Building a fully generic, configurable reporting engine before any real client has
asked for a second bespoke report would be exactly the premature abstraction
`RULES.md` warns against. This extension point is intentionally the simplest thing
that works: one interface, one lookup-by-key use case. If several schools end up
asking for genuinely similar bespoke reports, *that* repeated pattern — not a guess
made now — is the signal to extract something more general (and a candidate for
`SmartCampus.Core`, see `docs/ARCHITECTURE.md` §9).
