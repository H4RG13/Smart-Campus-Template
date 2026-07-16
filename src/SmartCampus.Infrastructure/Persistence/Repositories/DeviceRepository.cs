using Microsoft.EntityFrameworkCore;
using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Repositories;

public sealed class DeviceRepository(AppDbContext dbContext) : IDeviceRepository
{
    public void Add(Device device) => dbContext.Devices.Add(device);

    public Task<Device?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Devices.SingleOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task<bool> HardwareIdExistsAsync(string hardwareId, CancellationToken cancellationToken = default) =>
        dbContext.Devices.AnyAsync(d => d.HardwareId == hardwareId, cancellationToken);

    public async Task<IReadOnlyList<Device>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Devices.OrderBy(d => d.DeviceName).ToListAsync(cancellationToken);
}
