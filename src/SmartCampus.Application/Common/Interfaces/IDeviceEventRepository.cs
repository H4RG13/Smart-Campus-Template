using SmartCampus.Domain.Entities;

namespace SmartCampus.Application.Common.Interfaces;

public interface IDeviceEventRepository
{
    void Add(DeviceEvent deviceEvent);
    Task<bool> ExistsAsync(Guid deviceId, string deviceEventId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeviceEvent>> ListByDeviceAsync(Guid deviceId, CancellationToken cancellationToken = default);
}
