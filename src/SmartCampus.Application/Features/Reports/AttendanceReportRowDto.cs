namespace SmartCampus.Application.Features.Reports;

public sealed class AttendanceReportRowDto
{
    public required Guid StudentId { get; init; }
    public required string StudentName { get; init; }
    public required string StudentNumber { get; init; }
    public required int TotalDays { get; init; }
    public required int OnTimeCount { get; init; }
    public required int LateCount { get; init; }
    public required int AbsentCount { get; init; }
    public required int ExcusedAbsenceCount { get; init; }
    public required double AttendanceRate { get; init; }
}
