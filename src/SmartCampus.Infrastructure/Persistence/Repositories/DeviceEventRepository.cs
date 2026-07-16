using Microsoft.EntityFrameworkCore;
using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Repositories;

public sealed class DeviceEventRepository(AppDbContext dbContext) : IDeviceEventRepository
{
    public void Add(DeviceEvent deviceEvent) => dbContext.DeviceEvents.Add(deviceEvent);

    public Task<bool> ExistsAsync(Guid deviceId, string deviceEventId, CancellationToken cancellationToken = default) =>
        dbContext.DeviceEvents.AnyAsync(e => e.DeviceId == deviceId && e.DeviceEventId == deviceEventId, cancellationToken);

    public async Task<IReadOnlyList<DeviceEvent>> ListByDeviceAsync(Guid deviceId, CancellationToken cancellationToken = default) =>
        await dbContext.DeviceEvents
            .Where(e => e.DeviceId == deviceId)
            .OrderByDescending(e => e.ReceivedAtUtc)
            .Take(100)
            .ToListAsync(cancellationToken);
}
