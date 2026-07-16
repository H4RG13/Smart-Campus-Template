using SmartCampus.Domain.Entities;

namespace SmartCampus.Application.Common.Interfaces;

public interface IDeviceHeartbeatRepository
{
    void Add(DeviceHeartbeat heartbeat);

    /// <summary>Latest heartbeat timestamp per device — used to compute online/offline status.</summary>
    Task<IReadOnlyDictionary<Guid, DateTime>> GetLatestReceivedAtByDeviceAsync(CancellationToken cancellationToken = default);
}
