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
    public string? Notes { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private AttendanceRecord() { }

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

    public void Correct(AttendanceStatus newStatus, string? notes)
    {
        Status = newStatus;
        Notes = notes;
    }
}
