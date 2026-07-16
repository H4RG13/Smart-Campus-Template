using Microsoft.EntityFrameworkCore;
using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Repositories;

public sealed class NotificationLogRepository(AppDbContext dbContext) : INotificationLogRepository
{
    public void Add(NotificationLog log) => dbContext.NotificationLogs.Add(log);

    public async Task<IReadOnlyList<NotificationLog>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbContext.NotificationLogs
            .OrderByDescending(n => n.SentAtUtc)
            .Take(200)
            .ToListAsync(cancellationToken);
}
