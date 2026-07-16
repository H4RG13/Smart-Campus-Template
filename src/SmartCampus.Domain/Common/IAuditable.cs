namespace SmartCampus.Domain.Common;

/// <summary>
/// Marks an entity whose Add/Update operations are recorded to the audit trail
/// (see docs/LOGGING.md — the audit trail is a business record, not an operational log).
/// </summary>
public interface IAuditable
{
    Guid Id { get; }
}
