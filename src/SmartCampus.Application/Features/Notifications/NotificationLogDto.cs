using SmartCampus.Domain.Enums;

namespace SmartCampus.Application.Features.Notifications;

public sealed class NotificationLogDto
{
    public required Guid Id { get; init; }
    public required Guid StudentId { get; init; }
    public required Guid GuardianId { get; init; }
    public required NotificationChannel Channel { get; init; }
    public required NotificationTriggerReason TriggerReason { get; init; }
    public required NotificationStatus Status { get; init; }
    public required DateTime SentAtUtc { get; init; }
}
