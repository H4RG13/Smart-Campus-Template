# Database Design

PostgreSQL, one database per school (per the independent-deployment model). Schema shown here is the V1 baseline — every table exists in every school's database; per-school *values* differ, not structure (Rule #2 in `RULES.md`).

## Design principles

- **Soft delete over hard delete** for anything referenced by historical records (Students, Staff, Devices) — attendance history must remain meaningful even after a student leaves. A `IsActive` / `DeactivatedAtUtc` column, not a physical `DELETE`.
- **UTC everywhere in storage.** `SchoolSettings.Timezone` is used only for display/formatting at the edges (API responses, UI), never for stored timestamps.
- **Audit trail as a cross-cutting interceptor**, not a bespoke table per entity — one `AuditLogs` table capturing entity type, entity id, action, changed-by, timestamp, and a JSON diff.
- **No multi-tenancy columns.** There is no `SchoolId` / `TenantId` column anywhere — one school, one database. Adding one would be solving a problem this model doesn't have (and would violate Rule #2's spirit at the schema level).

## Entity Relationship Diagram

```mermaid
erDiagram
    USERS ||--o{ USER_ROLES : has
    ROLES ||--o{ USER_ROLES : assigned_to
    USERS ||--o| STAFF : "is (optional)"
    USERS ||--o| STUDENTS : "is (optional)"
    USERS ||--o| GUARDIANS : "is (optional)"

    STUDENTS }o--|| CLASSES : enrolled_in
    STUDENTS ||--o{ GUARDIAN_STUDENT : has
    GUARDIANS ||--o{ GUARDIAN_STUDENT : guardian_of

    CLASSES }o--|| ACADEMIC_TERMS : belongs_to
    CLASSES ||--o{ CLASS_STAFF : taught_by
    STAFF ||--o{ CLASS_STAFF : teaches

    ACADEMIC_TERMS ||--o{ CALENDAR_EXCEPTIONS : has

    STUDENTS ||--o{ ATTENDANCE_RECORDS : has
    ATTENDANCE_RECORDS }o--o| DEVICE_EVENTS : "originated from (optional)"
    ATTENDANCE_RECORDS }o--o| USERS : "recorded by (optional, manual entry)"

    DEVICES ||--o{ DEVICE_EVENTS : produces
    DEVICES ||--o{ DEVICE_HEARTBEATS : sends

    STUDENTS ||--o{ NOTIFICATION_LOGS : "notification about"
    GUARDIANS ||--o{ NOTIFICATION_LOGS : "notification to"

    USERS ||--o{ AUDIT_LOGS : "performed by"

    USERS {
        guid Id PK
        string Email UK
        string PasswordHash
        bool IsActive
        timestamptz CreatedAtUtc
        timestamptz DeactivatedAtUtc
    }
    ROLES {
        guid Id PK
        string Name UK
    }
    USER_ROLES {
        guid UserId FK
        guid RoleId FK
    }
    STUDENTS {
        guid Id PK
        guid UserId FK "nullable, if student has portal login"
        string FirstName
        string LastName
        string StudentNumber UK
        guid ClassId FK
        date DateOfBirth
        string RfidTagId "nullable, assigned card"
        bool IsActive
        timestamptz EnrolledAtUtc
        timestamptz DeactivatedAtUtc
    }
    STAFF {
        guid Id PK
        guid UserId FK
        string FirstName
        string LastName
        string EmployeeNumber UK
        string Position
        bool IsActive
    }
    GUARDIANS {
        guid Id PK
        guid UserId FK "nullable"
        string FirstName
        string LastName
        string Phone
        string Email
    }
    GUARDIAN_STUDENT {
        guid GuardianId FK
        guid StudentId FK
        string Relationship
    }
    CLASSES {
        guid Id PK
        string Name
        guid AcademicTermId FK
    }
    CLASS_STAFF {
        guid ClassId FK
        guid StaffId FK
        string RoleInClass "e.g. HomeroomTeacher"
    }
    ACADEMIC_TERMS {
        guid Id PK
        string Name
        date StartDate
        date EndDate
        bool IsActive
    }
    CALENDAR_EXCEPTIONS {
        guid Id PK
        guid AcademicTermId FK
        date Date
        string Type "Holiday / HalfDay / SpecialSchedule"
        string Description
    }
    ATTENDANCE_RECORDS {
        guid Id PK
        guid StudentId FK
        date AttendanceDate
        timestamptz CheckInAtUtc
        string Status "OnTime / Late / Absent / ExcusedAbsence"
        guid RecordedByUserId FK "nullable, null if device-originated"
        guid DeviceEventId FK "nullable, null if manual"
        string Notes
        timestamptz CreatedAtUtc
    }
    DEVICES {
        guid Id PK
        string DeviceName
        string HardwareId UK
        string AuthTokenHash
        string FirmwareVersion
        bool IsActive
        timestamptz RegisteredAtUtc
        string Location
    }
    DEVICE_EVENTS {
        guid Id PK
        guid DeviceId FK
        string DeviceEventId UK "for idempotency/dedup"
        string RfidTagId
        timestamptz DeviceTimestampUtc
        timestamptz ReceivedAtUtc
        string ProcessingStatus "Accepted / RejectedDuplicate / RejectedUnknownTag / RejectedInvalidTimestamp"
    }
    DEVICE_HEARTBEATS {
        guid Id PK
        guid DeviceId FK
        timestamptz ReceivedAtUtc
        string FirmwareVersion
        int SignalStrength
        int QueueDepth "pending offline-queued events on device"
    }
    NOTIFICATION_LOGS {
        guid Id PK
        guid StudentId FK
        guid GuardianId FK
        string Channel "Email / SMS"
        string TriggerReason "Absence / Late"
        string Status "Sent / Failed"
        timestamptz SentAtUtc
    }
    AUDIT_LOGS {
        guid Id PK
        string EntityType
        guid EntityId
        string Action "Create / Update / Delete"
        guid PerformedByUserId FK
        jsonb Changes
        timestamptz OccurredAtUtc
    }
```

## Notes on key design decisions

**`DEVICE_EVENTS` is a separate table from `ATTENDANCE_RECORDS`.** A device event is a raw, untrusted signal; an attendance record is the Attendance Engine's *decision* after applying business rules (lateness threshold, calendar exceptions, duplicate debounce). Keeping them separate means:
- A rejected/duplicate scan is still logged (useful for debugging device issues) without polluting attendance history.
- The Attendance Engine's rules can change and be *reapplied* conceptually without needing to have mutated raw device data.

**`DeviceEventId` (string, from firmware) is distinct from the row's own `Id`.** The firmware generates its own idempotency key (e.g. a local sequence number + device ID) so that a retried offline-queued event that already succeeded is recognized as a duplicate server-side, not double-counted.

**`AttendanceRecords.RecordedByUserId` and `DeviceEventId` are both nullable, mutually exclusive-in-practice.** A record is either device-originated or manually entered by staff; a `CHECK` constraint enforces exactly one is set, giving a clean audit answer to "how did this record get here" without a polymorphic table.

**`GUARDIAN_STUDENT` is many-to-many.** A student can have multiple guardians (both parents, a legal guardian); a guardian can have multiple students (siblings) — common enough in real school data to model correctly from V1 rather than retrofit.

**Indexes to add at migration time (not exhaustive, but load-bearing):**
- `AttendanceRecords (StudentId, AttendanceDate)` — the dominant query pattern (a student's attendance over a date range).
- `DeviceEvents (DeviceId, DeviceEventId)` unique — enforces idempotency at the database level, not just application logic.
- `Students (RfidTagId)` — device ingestion looks up a student by scanned tag on every event.
