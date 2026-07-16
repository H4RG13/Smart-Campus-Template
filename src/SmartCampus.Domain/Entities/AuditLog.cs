namespace SmartCampus.Domain.Entities;

public sealed class AuditLog
{
    public Guid Id { get; private set; }
    public string EntityType { get; private set; } = null!;
    public Guid EntityId { get; private set; }
    public string Action { get; private set; } = null!;
    public Guid? PerformedByUserId { get; private set; }
    public string Changes { get; private set; } = null!;
    public DateTime OccurredAtUtc { get; private set; }

    private AuditLog() { }

    public AuditLog(string entityType, Guid entityId, string action, Guid? performedByUserId, string changes, DateTime occurredAtUtc)
    {
        Id = Guid.NewGuid();
        EntityType = entityType;
        EntityId = entityId;
        Action = action;
        PerformedByUserId = performedByUserId;
        Changes = changes;
        OccurredAtUtc = occurredAtUtc;
    }
}
