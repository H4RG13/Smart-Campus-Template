# SmartCampus Template

A reusable, white-labelable school management system template. Cloned and independently deployed per school client — not a SaaS, not multi-tenant. Each school gets its own repository, database, domain, server, and configuration.

## What this is

SmartCampus Template is the base codebase our agency clones for every new school client. It covers the operational core every school shares — students, staff, attendance (including RFID-based check-in via ESP32 devices), academic calendar, notifications, RBAC — while keeping everything school-specific (branding, unique policies, custom reports) in configuration or clearly isolated extension points, never in forked core logic.

See [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) for the full architectural rationale (vision, layering, module breakdown, decisions and trade-offs) and [`RULES.md`](RULES.md) for the non-negotiable engineering guardrails.

## Tech stack

- **Backend:** ASP.NET Core (.NET 9), EF Core, PostgreSQL, Redis (optional), JWT + RBAC, SignalR, Serilog, FluentValidation, Swagger
- **Frontend:** React, TypeScript, Vite, TailwindCSS, TanStack Query, React Hook Form, React Router, Zustand, Apache ECharts
- **Infrastructure:** Docker, Docker Compose, NGINX, GitHub Actions, HTTPS
- **Storage:** MinIO (or S3-compatible), abstracted behind `IFileStorage`
- **IoT:** ESP32 + MFRC522 RFID reader (firmware in [`firmware/`](firmware/))

## Repository layout

```
src/
  SmartCampus.Domain/          Entities, value objects, business invariants
  SmartCampus.Application/     Use cases, feature slices, validation, ports
  SmartCampus.Infrastructure/  EF Core, Postgres, Redis, MinIO, SMTP, IoT ingestion
  SmartCampus.Api/             Controllers, SignalR hubs, middleware
  SmartCampus.Configuration/   Strongly-typed per-school config models
client/                        React frontend
firmware/esp32-rfid/           ESP32 firmware (RFID read, offline queue, heartbeat)
deploy/                        docker-compose.yml, NGINX config, .env.example
tests/
docs/
```

## Getting started (local development)

> Full setup steps land here once the solution scaffold exists (Phase 1 of `PLAN.md`).

1. Clone the repo.
2. Copy `deploy/.env.example` to `.env` and fill in local dev values.
3. `docker compose -f deploy/docker-compose.yml up -d postgres` (and `redis` if enabled).
4. `dotnet ef database update --project src/SmartCampus.Infrastructure --startup-project src/SmartCampus.Api`
5. `dotnet run --project src/SmartCampus.Api`
6. `cd client && npm install && npm run dev`

## Deploying a new school (agency workflow)

1. Requirement gathering
2. Create repository from `SmartCampus-Template`
3. Rename project (namespace, solution, container names — see checklist in `docs/`)
4. Configure branding (school name, logo, colors, timezone)
5. Configure environment variables (`.env`, connection strings, SMTP, API keys)
6. Provision the school's PostgreSQL database and run migrations
7. Configure and register the school's ESP32 devices
8. Deploy via Docker Compose + NGINX/SSL on the school's VPS
9. Train school staff
10. Ongoing maintenance (independent per school — one school going offline never affects another)

## Key principle: configuration over forking

Changing behavior for a specific school should mean changing a config value, not editing application logic. If you find yourself branching core logic on a school's identity, stop — that belongs in configuration or an isolated extension point. See `RULES.md`.

## Status

Template under active initial development. Not yet deployed to a production client.
