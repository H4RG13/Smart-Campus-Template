using SmartCampus.Domain.Entities;

namespace SmartCampus.Application.Common.Interfaces;

public interface IDeviceRepository
{
    void Add(Device device);
    Task<Device?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> HardwareIdExistsAsync(string hardwareId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Device>> ListAsync(CancellationToken cancellationToken = default);
}
