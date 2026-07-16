using SmartCampus.Domain.Enums;

namespace SmartCampus.Application.Features.Attendance;

public sealed class AttendanceRecordDto
{
    public required Guid Id { get; init; }
    public required Guid StudentId { get; init; }
    public required DateOnly AttendanceDate { get; init; }
    public DateTime? CheckInAtUtc { get; init; }
    public required AttendanceStatus Status { get; init; }
    public Guid? RecordedByUserId { get; init; }
    public string? Notes { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
}
