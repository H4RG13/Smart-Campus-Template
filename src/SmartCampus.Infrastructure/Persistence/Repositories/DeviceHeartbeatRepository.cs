using Microsoft.EntityFrameworkCore;
using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Repositories;

public sealed class DeviceHeartbeatRepository(AppDbContext dbContext) : IDeviceHeartbeatRepository
{
    public void Add(DeviceHeartbeat heartbeat) => dbContext.DeviceHeartbeats.Add(heartbeat);

    public async Task<IReadOnlyDictionary<Guid, DateTime>> GetLatestReceivedAtByDeviceAsync(CancellationToken cancellationToken = default)
    {
        var latest = await dbContext.DeviceHeartbeats
            .GroupBy(h => h.DeviceId)
            .Select(g => new { DeviceId = g.Key, LastReceivedAtUtc = g.Max(h => h.ReceivedAtUtc) })
            .ToListAsync(cancellationToken);

        return latest.ToDictionary(x => x.DeviceId, x => x.LastReceivedAtUtc);
    }
}
