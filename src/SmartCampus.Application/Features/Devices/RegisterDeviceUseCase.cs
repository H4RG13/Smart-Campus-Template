using System.Security.Cryptography;
using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Application.Features.Devices;

public sealed class RegisterDeviceUseCase(
    IDeviceRepository deviceRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
{
    public async Task<RegisterDeviceResponse> ExecuteAsync(RegisterDeviceRequest request, CancellationToken cancellationToken = default)
    {
        var authToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        var device = new Device(request.DeviceName, request.HardwareId, passwordHasher.Hash(authToken), request.Location, DateTime.UtcNow);
        deviceRepository.Add(device);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterDeviceResponse
        {
            DeviceId = device.Id,
            AuthToken = authToken,
            DeviceName = device.DeviceName,
            HardwareId = device.HardwareId,
        };
    }
}
