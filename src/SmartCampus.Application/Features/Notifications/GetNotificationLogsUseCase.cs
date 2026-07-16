using SmartCampus.Application.Common.Interfaces;

namespace SmartCampus.Application.Features.Notifications;

public sealed class GetNotificationLogsUseCase(INotificationLogRepository notificationLogRepository)
{
    public async Task<IReadOnlyList<NotificationLogDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var logs = await notificationLogRepository.ListAsync(cancellationToken);

        return logs.Select(l => new NotificationLogDto
        {
            Id = l.Id,
            StudentId = l.StudentId,
            GuardianId = l.GuardianId,
            Channel = l.Channel,
            TriggerReason = l.TriggerReason,
            Status = l.Status,
            SentAtUtc = l.SentAtUtc,
        }).ToList();
    }
}
