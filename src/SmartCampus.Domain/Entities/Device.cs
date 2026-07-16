namespace SmartCampus.Domain.Entities;

public sealed class Device
{
    public Guid Id { get; private set; }
    public string DeviceName { get; private set; } = null!;
    public string HardwareId { get; private set; } = null!;
    public string AuthTokenHash { get; private set; } = null!;
    public string? FirmwareVersion { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime RegisteredAtUtc { get; private set; }
    public string? Location { get; private set; }

    private Device() { }

    public Device(string deviceName, string hardwareId, string authTokenHash, string? location, DateTime registeredAtUtc)
    {
        if (string.IsNullOrWhiteSpace(deviceName))
        {
            throw new ArgumentException("Device name cannot be empty.", nameof(deviceName));
        }

        if (string.IsNullOrWhiteSpace(hardwareId))
        {
            throw new ArgumentException("Hardware id cannot be empty.", nameof(hardwareId));
        }

        Id = Guid.NewGuid();
        DeviceName = deviceName;
        HardwareId = hardwareId;
        AuthTokenHash = authTokenHash;
        Location = location;
        RegisteredAtUtc = registeredAtUtc;
        IsActive = true;
    }

    public void RecordFirmwareVersion(string firmwareVersion) => FirmwareVersion = firmwareVersion;

    public void Deactivate() => IsActive = false;
}
