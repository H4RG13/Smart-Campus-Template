using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Domain.Common;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Writes one AuditLog row per Add/Update of an IAuditable entity — see docs/LOGGING.md:
/// the audit trail is a business record, kept separate from operational (Serilog) logs.
/// </summary>
public sealed class AuditSaveChangesInterceptor(ICurrentUserAccessor currentUserAccessor) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            WriteAuditLogs(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void WriteAuditLogs(DbContext context)
    {
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditable && e.State is EntityState.Added or EntityState.Modified)
            .ToList();

        foreach (var entry in entries)
        {
            var auditable = (IAuditable)entry.Entity;
            var action = entry.State == EntityState.Added ? "Create" : "Update";
            var changes = BuildChangesJson(entry);

            var auditLog = new AuditLog(
                entry.Entity.GetType().Name,
                auditable.Id,
                action,
                currentUserAccessor.UserId,
                changes,
                DateTime.UtcNow);

            context.Set<AuditLog>().Add(auditLog);
        }
    }

    private static string BuildChangesJson(EntityEntry entry)
    {
        var changes = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (entry.State == EntityState.Added)
            {
                changes[property.Metadata.Name] = property.CurrentValue;
            }
            else if (property.IsModified)
            {
                changes[property.Metadata.Name] = new { Old = property.OriginalValue, New = property.CurrentValue };
            }
        }

        return JsonSerializer.Serialize(changes);
    }
}
