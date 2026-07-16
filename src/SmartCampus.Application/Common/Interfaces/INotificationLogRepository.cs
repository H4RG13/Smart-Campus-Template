using SmartCampus.Domain.Entities;

namespace SmartCampus.Application.Common.Interfaces;

public interface INotificationLogRepository
{
    void Add(NotificationLog log);
    Task<IReadOnlyList<NotificationLog>> ListAsync(CancellationToken cancellationToken = default);
}
