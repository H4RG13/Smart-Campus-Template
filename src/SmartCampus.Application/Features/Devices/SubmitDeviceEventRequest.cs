namespace SmartCampus.Application.Features.Devices;

public sealed class SubmitDeviceEventRequest
{
    public required string DeviceEventId { get; init; }
    public required string RfidTagId { get; init; }
    public required DateTime DeviceTimestampUtc { get; init; }
}
