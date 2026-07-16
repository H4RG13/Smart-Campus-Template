using SmartCampus.Application.Common.Exceptions;
using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Application.Features.Devices;

public sealed class SubmitHeartbeatUseCase(
    IDeviceRepository deviceRepository,
    IDeviceHeartbeatRepository heartbeatRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
{
    public async Task ExecuteAsync(
        Guid deviceId,
        string deviceAuthToken,
        SubmitHeartbeatRequest request,
        CancellationToken cancellationToken = default)
    {
        var device = await deviceRepository.GetByIdAsync(deviceId, cancellationToken);
        if (device is null || !device.IsActive || !passwordHasher.Verify(deviceAuthToken, device.AuthTokenHash))
        {
            throw new DeviceAuthenticationFailedException();
        }

        device.RecordFirmwareVersion(request.FirmwareVersion);
        heartbeatRepository.Add(new DeviceHeartbeat(deviceId, DateTime.UtcNow, request.FirmwareVersion, request.SignalStrength, request.QueueDepth));

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
