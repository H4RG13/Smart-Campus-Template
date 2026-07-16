# Clone-and-Deploy Checklist

The step-by-step version of the workflow in `README.md` and `docs/ARCHITECTURE.md` §10,
written so a developer who's never touched this template can follow it start to finish
with no undocumented manual steps — this is what `PLAN.md` Phase 6's exit criteria means
by "a fresh clone can go from requirement gathering to deployed and training-ready."

## 1. Requirement gathering

Confirm with the school: their domain name, branding (logo, colors), timezone,
attendance late-threshold policy, whether they want RFID devices in this initial
rollout, and who the admin contact is (for the seed account).

## 2. Create the repository

Use GitHub's "Use this template" (not a manual clone/copy) so the new repo has no
shared git history with the template — see `docs/ARCHITECTURE.md` §11 on why.

## 3. Rename the project

```bash
scripts/rename-project.sh <SchoolCodeName>
```

e.g. `scripts/rename-project.sh Riverside` turns `SmartCampus.Api` into
`Riverside.Api` across the solution, projects, namespaces, and container names.
**Verified against a throwaway clone**: both `dotnet build` and `cd client && npm run
build` succeed cleanly after running this script — see the Phase 6 verification notes
in `PLAN.md`.

This is a technical/code-identifier rename, distinct from branding (step 4) — a
school's display name and logo never need to touch a namespace.

## 4. Configure branding

Set in `deploy/.env` (see `deploy/.env.example` for every key):
`BRANDING__SCHOOLNAME`, `BRANDING__LOGOURL`, `BRANDING__PRIMARYCOLOR`,
`BRANDING__SECONDARYCOLOR`, `BRANDING__TIMEZONE`.

## 5. Configure environment variables

Copy `deploy/.env.example` to `deploy/.env` and fill in every value — connection
string pieces, `JWT_SIGNING_KEY` (generate a real random 256-bit value, never reuse
across schools), `SEED__ADMINEMAIL`/`SEED__ADMINPASSWORD`, SMTP credentials,
`DOMAIN_NAME`/`CERTBOT_EMAIL`. See `docs/SECRETS_MANAGEMENT.md` for what each of
these is and how to rotate it later.

## 6. Provision PostgreSQL

`docker compose up -d postgres` — migrations apply automatically on the `api`
container's first boot (see `docs/DEPLOYMENT_ARCHITECTURE.md`).

## 7. Configure IoT devices (if this school uses RFID at launch)

Register each device via `POST /devices/register` (Admin-authenticated), flash
`firmware/esp32-rfid/` with that device's id/token — see the firmware's own
README. Optional at initial launch; can be added in a later phase for this school.

## 8. Deploy

```bash
cd client && npm run build                 # produces client/dist, served by nginx
cd ../deploy
docker compose build api
docker compose up -d postgres api
./init-ssl.sh                              # one-time: bootstraps the Let's Encrypt cert
```

`init-ssl.sh` starts `nginx` itself as part of the bootstrap. After this, routine
redeploys are just `docker compose up -d --build api` (nginx/certbot keep running).

## 9. Verify

- `curl https://<domain>/api/v1/health` → `Healthy`
- Log in as the seed admin, confirm the school's branding renders (not the demo
  placeholder), change the seed admin password immediately.
- If devices are in scope for this school: confirm a test scan produces an
  attendance record and the device shows "online" on the dashboard.

## 10. Training

Standard agency process — walk the school's admin/teachers through the UI.

## 11. Maintenance

See `docs/MONITORING.md`, `docs/LOGGING.md`, and the backup/restore procedure in
`scripts/backup-postgres.sh` / `scripts/restore-postgres.sh`.

---

## What this checklist deliberately does not cover

- Full "white-labeling" of the documentation itself (`docs/*.md`, `README.md`
  mentioning "SmartCampus") — `scripts/rename-project.sh` intentionally skips these
  since they're internal engineering docs, not something the school or its users
  ever see. Update them by hand only if this fork is going to diverge significantly
  from the template's own documented architecture.
- Redis/MinIO — only added if a specific school's load or storage needs justify it
  (Rule #8); not part of the default rollout.
