namespace SmartCampus.Application.Features.Devices;

/// <summary>The AuthToken is returned once, at registration, and never again — see RULES.md #5.</summary>
public sealed class RegisterDeviceResponse
{
    public required Guid DeviceId { get; init; }
    public required string AuthToken { get; init; }
    public required string DeviceName { get; init; }
    public required string HardwareId { get; init; }
}
