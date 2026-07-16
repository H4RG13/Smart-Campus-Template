using SmartCampus.Domain.Enums;

namespace SmartCampus.Application.Features.Attendance;

/// <summary>
/// Doubles as both "manual check-in" (CheckInTime set, Status computed by
/// AttendanceStatusCalculator) and "manual override" for absences (CheckInTime
/// omitted, Status explicitly Absent/ExcusedAbsence) — one endpoint covering both
/// bullet points from PLAN.md Phase 3 rather than two near-identical ones.
/// </summary>
public sealed class RecordAttendanceRequest
{
    public required Guid StudentId { get; init; }
    public required DateOnly AttendanceDate { get; init; }
    public TimeOnly? CheckInTime { get; init; }
    public AttendanceStatus? Status { get; init; }
    public string? Notes { get; init; }
}
