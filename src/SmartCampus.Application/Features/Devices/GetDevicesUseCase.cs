using Microsoft.Extensions.Options;
using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Configuration;

namespace SmartCampus.Application.Features.Devices;

public sealed class GetDevicesUseCase(
    IDeviceRepository deviceRepository,
    IDeviceHeartbeatRepository heartbeatRepository,
    IOptions<IotSettings> iotSettings)
{
    public async Task<IReadOnlyList<DeviceDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var devices = await deviceRepository.ListAsync(cancellationToken);
        var lastHeartbeats = await heartbeatRepository.GetLatestReceivedAtByDeviceAsync(cancellationToken);

        var offlineThreshold = TimeSpan.FromSeconds(iotSettings.Value.HeartbeatIntervalSeconds * iotSettings.Value.OfflineThresholdMultiplier);
        var now = DateTime.UtcNow;

        return devices.Select(device =>
        {
            lastHeartbeats.TryGetValue(device.Id, out var lastHeartbeatAtUtc);
            var hasHeartbeat = lastHeartbeats.ContainsKey(device.Id);

            return new DeviceDto
            {
                Id = device.Id,
                DeviceName = device.DeviceName,
                HardwareId = device.HardwareId,
                FirmwareVersion = device.FirmwareVersion,
                IsActive = device.IsActive,
                RegisteredAtUtc = device.RegisteredAtUtc,
                Location = device.Location,
                LastHeartbeatAtUtc = hasHeartbeat ? lastHeartbeatAtUtc : null,
                IsOnline = hasHeartbeat && now - lastHeartbeatAtUtc <= offlineThreshold,
            };
        }).ToList();
    }
}
