namespace SmartCampus.Application.Features.Devices;

public sealed class DeviceDto
{
    public required Guid Id { get; init; }
    public required string DeviceName { get; init; }
    public required string HardwareId { get; init; }
    public string? FirmwareVersion { get; init; }
    public required bool IsActive { get; init; }
    public required DateTime RegisteredAtUtc { get; init; }
    public string? Location { get; init; }
    public DateTime? LastHeartbeatAtUtc { get; init; }
    public required bool IsOnline { get; init; }
}
