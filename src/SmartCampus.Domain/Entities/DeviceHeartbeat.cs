namespace SmartCampus.Domain.Entities;

public sealed class DeviceHeartbeat
{
    public Guid Id { get; private set; }
    public Guid DeviceId { get; private set; }
    public DateTime ReceivedAtUtc { get; private set; }
    public string FirmwareVersion { get; private set; } = null!;
    public int SignalStrength { get; private set; }
    public int QueueDepth { get; private set; }

    private DeviceHeartbeat() { }

    public DeviceHeartbeat(Guid deviceId, DateTime receivedAtUtc, string firmwareVersion, int signalStrength, int queueDepth)
    {
        Id = Guid.NewGuid();
        DeviceId = deviceId;
        ReceivedAtUtc = receivedAtUtc;
        FirmwareVersion = firmwareVersion;
        SignalStrength = signalStrength;
        QueueDepth = queueDepth;
    }
}
