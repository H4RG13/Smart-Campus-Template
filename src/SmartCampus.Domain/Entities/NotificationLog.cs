using SmartCampus.Domain.Enums;

namespace SmartCampus.Domain.Entities;

public sealed class NotificationLog
{
    public Guid Id { get; private set; }
    public Guid StudentId { get; private set; }
    public Guid GuardianId { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public NotificationTriggerReason TriggerReason { get; private set; }
    public NotificationStatus Status { get; private set; }
    public DateTime SentAtUtc { get; private set; }

    private NotificationLog() { }

    public NotificationLog(
        Guid studentId,
        Guid guardianId,
        NotificationChannel channel,
        NotificationTriggerReason triggerReason,
        NotificationStatus status,
        DateTime sentAtUtc)
    {
        Id = Guid.NewGuid();
        StudentId = studentId;
        GuardianId = guardianId;
        Channel = channel;
        TriggerReason = triggerReason;
        Status = status;
        SentAtUtc = sentAtUtc;
    }
}
