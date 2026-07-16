namespace SmartCampus.Application.Features.Attendance;

public sealed class AttendanceDaySummaryDto
{
    public required DateOnly Date { get; init; }
    public required int OnTimeCount { get; init; }
    public required int LateCount { get; init; }
    public required int AbsentCount { get; init; }
    public required int ExcusedAbsenceCount { get; init; }
}
