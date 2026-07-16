using SmartCampus.Domain.Enums;

namespace SmartCampus.Domain.Entities;

public sealed class DeviceEvent
{
    public Guid Id { get; private set; }
    public Guid DeviceId { get; private set; }
    public string DeviceEventId { get; private set; } = null!;
    public string RfidTagId { get; private set; } = null!;
    public DateTime DeviceTimestampUtc { get; private set; }
    public DateTime ReceivedAtUtc { get; private set; }
    public DeviceEventProcessingStatus ProcessingStatus { get; private set; }

    private DeviceEvent() { }

    public DeviceEvent(
        Guid deviceId,
        string deviceEventId,
        string rfidTagId,
        DateTime deviceTimestampUtc,
        DateTime receivedAtUtc,
        DeviceEventProcessingStatus processingStatus)
    {
        if (string.IsNullOrWhiteSpace(deviceEventId))
        {
            throw new ArgumentException("Device event id cannot be empty.", nameof(deviceEventId));
        }

        if (string.IsNullOrWhiteSpace(rfidTagId))
        {
            throw new ArgumentException("RFID tag cannot be empty.", nameof(rfidTagId));
        }

        Id = Guid.NewGuid();
        DeviceId = deviceId;
        DeviceEventId = deviceEventId;
        RfidTagId = rfidTagId;
        DeviceTimestampUtc = deviceTimestampUtc;
        ReceivedAtUtc = receivedAtUtc;
        ProcessingStatus = processingStatus;
    }
}
