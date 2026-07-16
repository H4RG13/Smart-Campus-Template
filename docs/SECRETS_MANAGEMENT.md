# Secrets Management

A record of what counts as a secret in this template, where each one lives, and the
audit performed before the first real deployment. See `RULES.md` and
`docs/DEPLOYMENT_ARCHITECTURE.md` for the surrounding policy this implements.

## What counts as a secret here

| Secret | Where it's used | Generated |
|---|---|---|
| `JWT_SIGNING_KEY` | Signs/validates staff login tokens | Once per school, at setup |
| `POSTGRES_PASSWORD` | Database connection | Once per school, at setup |
| `SEED__ADMINPASSWORD` | First admin login | Once per school, changed immediately after first login |
| `SMTP__PASSWORD` | Guardian absence emails | Provided by the school's email provider |
| Per-device auth token | ESP32 → API authentication | Generated server-side at `POST /devices/register`, shown once |

## Where they live

- **Local development:** `deploy/.env` (gitignored) — copied from `deploy/.env.example`,
  which documents every key but contains no real values, only placeholders like
  `replace-with-a-real-256-bit-secret`.
- **Production (per school):** the same `deploy/.env` file, populated with that
  school's real values, living only on that school's VPS — never in the repo, never
  shared between schools (per the "nothing shared at runtime" principle in
  `docs/ARCHITECTURE.md`).
- **Firmware:** `firmware/esp32-rfid/include/Config.h` (gitignored, copied from
  `Config.h.example`) holds the WiFi password and device auth token, flashed
  directly onto that one device.

## Audit performed before Phase 6

Ran `git ls-files | grep -iE "\.env$|secret|credential|\.pem$|\.key$|\.pfx$"` against
the tracked file list — zero matches. Confirmed:

- `appsettings.json` and `appsettings.Development.json` contain only clearly-labeled
  dev-only demo values (`dev-only-signing-key-replace-with-a-real-256-bit-secret`,
  a seeded demo admin password) — safe because this repo is a **template**, not a
  live school's deployment; every real deployment overrides these via `.env`.
- `.gitignore` excludes `deploy/.env`, `.env`, `.env.local`, and
  `firmware/esp32-rfid/include/Config.h`.
- No `.pem`/`.key`/`.pfx` files are tracked anywhere in the repo.

**Re-run this audit** (the grep command above) before every release tag, and
whenever a new configuration section is added — it's cheap and catches the
"accidentally committed a real value while testing locally" mistake early.

## Rotation

- **JWT signing key:** rotating it invalidates all currently-issued tokens
  (everyone logged in is signed out) — do this during a maintenance window, not
  silently.
- **Device auth tokens:** rotate per-device by deactivating the old device record
  and registering a new one (re-flashing that device with the new token). There is
  currently no in-place token rotation endpoint — see `docs/FUTURE_MODULES.md` if
  fleet-wide rotation without a re-flash ever becomes a real requirement.
- **SMTP/DB credentials:** rotate per the provider's own process, then update that
  school's `.env` and restart the `api` container (`docker compose up -d api`).

## Docker secrets (optional upgrade path, not used by default)

V1 uses plain environment variables via `.env` + `docker-compose.yml`, which is
sufficient for a single-VPS-per-school deployment where the `.env` file itself is
access-controlled at the filesystem level. If a specific school's compliance
requirements demand it, Docker Compose supports file-based secrets
(`secrets:` top-level key, mounted at `/run/secrets/*` instead of passed as plain
env vars) — this is a per-school opt-in, not a template-wide default, per Rule #8
("no new infrastructure without an observed need").
