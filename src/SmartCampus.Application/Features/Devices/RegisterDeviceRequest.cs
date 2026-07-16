namespace SmartCampus.Application.Features.Devices;

public sealed class RegisterDeviceRequest
{
    public required string DeviceName { get; init; }
    public required string HardwareId { get; init; }
    public string? Location { get; init; }
}
