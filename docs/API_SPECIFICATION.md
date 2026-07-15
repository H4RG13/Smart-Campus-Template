# API Specification

REST over HTTPS, JSON, versioned via URL prefix (`/api/v1/...`). This document defines conventions and the surface area per module — not a full OpenAPI file (that's generated from Swagger/Swashbuckle directly off the controllers, and should be treated as the executable source of truth once code exists; this document is the design-time contract).

## Conventions

- **Base path:** `/api/v1`
- **Auth:** `Authorization: Bearer <JWT>` on every endpoint except `/api/v1/auth/login` and the public `/api/v1/config` (branding) and `/api/v1/health` endpoints.
- **Content type:** `application/json` request/response bodies.
- **Pagination:** `?page=1&pageSize=25` query params on list endpoints; response envelope includes `totalCount`, `page`, `pageSize`.
- **Errors:** RFC 7807 Problem Details format (`application/problem+json`) — `type`, `title`, `status`, `detail`, `errors` (field-level validation errors from FluentValidation).
- **Naming:** plural nouns for collections (`/students`, `/attendance-records`), kebab-case in URLs, camelCase in JSON bodies.
- **Idempotency:** device-facing ingestion endpoints require a client-supplied `deviceEventId` to guarantee safe retries (see IoT section below).

## Auth & Identity — `/api/v1/auth`

| Method | Path | Purpose | Auth |
|---|---|---|---|
| POST | `/auth/login` | Exchange email/password for JWT | Public |
| POST | `/auth/refresh` | Exchange refresh token for new JWT | Public (valid refresh token) |
| POST | `/auth/logout` | Invalidate refresh token | Authenticated |
| GET | `/auth/me` | Current user profile + roles | Authenticated |

## Configuration — `/api/v1/config`

| Method | Path | Purpose | Auth |
|---|---|---|---|
| GET | `/config/branding` | School name, logo URL, theme colors, timezone — consumed by frontend at boot | Public |

Kept public and minimal by design — the frontend needs branding before a user logs in (login page itself is branded).

## Students — `/api/v1/students`

| Method | Path | Purpose | Roles |
|---|---|---|---|
| GET | `/students` | List/search/paginate | Admin, Teacher, Staff |
| GET | `/students/{id}` | Detail | Admin, Teacher, Staff (own class only), Guardian (own children only) |
| POST | `/students` | Create/enroll | Admin |
| PUT | `/students/{id}` | Update profile | Admin |
| DELETE | `/students/{id}` | Deactivate (soft delete) | Admin |
| POST | `/students/{id}/rfid-tag` | Assign/replace RFID tag | Admin, Staff |

## Staff — `/api/v1/staff`

Mirrors Students' shape (`GET` list/detail, `POST`, `PUT`, `DELETE`), Admin-only for writes.

## Classes & Academic Calendar — `/api/v1/classes`, `/api/v1/academic-terms`

| Method | Path | Purpose | Roles |
|---|---|---|---|
| GET | `/classes` | List classes (optionally by term) | Admin, Teacher, Staff |
| POST | `/classes` | Create class/section | Admin |
| GET | `/academic-terms` | List terms | All authenticated |
| POST | `/academic-terms` | Create term | Admin |
| POST | `/academic-terms/{id}/exceptions` | Add holiday/exception date | Admin |

## Attendance — `/api/v1/attendance-records`

| Method | Path | Purpose | Roles |
|---|---|---|---|
| GET | `/attendance-records` | List, filterable by student/class/date range | Admin, Teacher, Staff, Guardian (own children) |
| POST | `/attendance-records` | Manual check-in / override | Admin, Teacher |
| PUT | `/attendance-records/{id}` | Correct a record (audited) | Admin |
| GET | `/attendance-records/summary` | Aggregate stats (daily/weekly rate) for dashboard charts | Admin, Teacher, Staff |

Manual entry (`POST`) and device-originated entry (below) both funnel through the same Application-layer Attendance Engine — the API surface differs because the caller/auth model differs, not because the business rule differs (Rule #4).

## IoT / Device Ingestion — `/api/v1/devices`

| Method | Path | Purpose | Auth |
|---|---|---|---|
| POST | `/devices/register` | Provision a new device, returns device auth token (one-time, done by admin during setup) | Admin |
| POST | `/devices/{deviceId}/events` | Submit an RFID scan event | Device token |
| POST | `/devices/{deviceId}/heartbeat` | Report health, firmware version, queue depth | Device token |
| GET | `/devices` | List devices + last-seen status | Admin, Staff |
| GET | `/devices/{deviceId}/events` | Recent event history for a device (debugging) | Admin |

**`POST /devices/{deviceId}/events` request body:**
```json
{
  "deviceEventId": "esp32-A1-000042",
  "rfidTagId": "04A3B2C1",
  "deviceTimestampUtc": "2026-07-14T07:58:03Z"
}
```
Server validates the device token, checks `deviceEventId` for duplicates (idempotent replay-safe for the firmware's offline-queue retry), resolves the tag to a student, and hands off to the Attendance Engine. Response includes the resulting attendance status so firmware can drive LED/buzzer feedback:
```json
{ "status": "OnTime", "studentDisplayName": "J. Rivera" }
```
On rejection (unknown tag, duplicate, invalid timestamp), response still returns 200 with a `processingStatus` field (not a 4xx) — a scan event is a fact that was received, not a client request error; the firmware needs a defined response either way to drive feedback.

## Notifications — `/api/v1/notifications`

| Method | Path | Purpose | Roles |
|---|---|---|---|
| GET | `/notifications/logs` | Audit view of sent notifications | Admin |
| POST | `/notifications/test` | Send a test notification (verify SMTP config after setup) | Admin |

## Reports — `/api/v1/reports`

| Method | Path | Purpose | Roles |
|---|---|---|---|
| GET | `/reports/attendance` | Standard attendance report, filterable, exportable (`?format=csv`) | Admin, Teacher |
| GET | `/reports/custom/{reportKey}` | Per-school bespoke reports (registered per deployment) | Admin |

`custom/{reportKey}` is the deliberate, isolated extension point referenced in `RULES.md` #9 — new bespoke reports are added here, never by branching a standard report's logic.

## Real-time — SignalR Hub `/hubs/live-attendance`

Not REST, but part of the API surface: broadcasts attendance/device-status events to connected dashboards (e.g. front-desk screen showing check-ins as they happen). Read-only push channel; all writes still go through the REST endpoints above — SignalR is not used as a command channel, only a notification channel (keeps the "one way to make a change" principle intact).

## Versioning policy

`/api/v1` is fixed for the lifetime of a school's deployment. Since each school runs its own backend and frontend deployed together, there's no cross-version compatibility problem to solve (unlike a multi-tenant SaaS) — a breaking change simply ships as part of that school's next deploy. `v2` is reserved for a genuinely incompatible redesign, not routine iteration.
