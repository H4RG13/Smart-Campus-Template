using SmartCampus.Domain.Enums;

namespace SmartCampus.Application.Features.Attendance;

public sealed class CorrectAttendanceRequest
{
    public required AttendanceStatus Status { get; init; }
    public string? Notes { get; init; }
}
