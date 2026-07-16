using SmartCampus.Domain.Common;
using SmartCampus.Domain.Enums;

namespace SmartCampus.Domain.Entities;

public sealed class AttendanceRecord : IAuditable
{
    public Guid Id { get; private set; }
    public Guid StudentId { get; private set; }
    public DateOnly AttendanceDate { get; private set; }
    public DateTime? CheckInAtUtc { get; private set; }
    public AttendanceStatus Status { get; private set; }
    public Guid? RecordedByUserId { get; private set; }
    public Guid? DeviceEventId { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private AttendanceRecord() { }

    /// <summary>Manual entry — recorded by a staff member (Phase 3).</summary>
    public AttendanceRecord(
        Guid studentId,
        DateOnly attendanceDate,
        AttendanceStatus status,
        DateTime? checkInAtUtc,
        Guid recordedByUserId,
        string? notes,
        DateTime createdAtUtc)
    {
        if ((status is AttendanceStatus.OnTime or AttendanceStatus.Late) && checkInAtUtc is null)
        {
            throw new ArgumentException("A check-in time is required for OnTime/Late status.", nameof(checkInAtUtc));
        }

        Id = Guid.NewGuid();
        StudentId = studentId;
        AttendanceDate = attendanceDate;
        Status = status;
        CheckInAtUtc = checkInAtUtc;
        RecordedByUserId = recordedByUserId;
        Notes = notes;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>
    /// Device-originated entry (Phase 4) — an RFID scan. Always has a check-in time and
    /// is only ever OnTime/Late: a device reports "this student is here," never "absent."
    /// Uses the same AttendanceStatusCalculator as manual entry — see RULES.md #4.
    /// </summary>
    public static AttendanceRecord FromDeviceEvent(
        Guid studentId,
        DateOnly attendanceDate,
        AttendanceStatus status,
        DateTime checkInAtUtc,
        Guid deviceEventId,
        DateTime createdAtUtc)
    {
        if (status is not (AttendanceStatus.OnTime or AttendanceStatus.Late))
        {
            throw new ArgumentException("A device-originated record can only be OnTime or Late.", nameof(status));
        }

        return new AttendanceRecord
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            AttendanceDate = attendanceDate,
            Status = status,
            CheckInAtUtc = checkInAtUtc,
            DeviceEventId = deviceEventId,
            CreatedAtUtc = createdAtUtc,
        };
    }

    public void Correct(AttendanceStatus newStatus, string? notes)
    {
        Status = newStatus;
        Notes = notes;
    }
}
