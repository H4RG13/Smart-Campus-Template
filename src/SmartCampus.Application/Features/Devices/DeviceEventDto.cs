using SmartCampus.Domain.Enums;

namespace SmartCampus.Application.Features.Devices;

public sealed class DeviceEventDto
{
    public required Guid Id { get; init; }
    public required string DeviceEventId { get; init; }
    public required string RfidTagId { get; init; }
    public required DateTime DeviceTimestampUtc { get; init; }
    public required DateTime ReceivedAtUtc { get; init; }
    public required DeviceEventProcessingStatus ProcessingStatus { get; init; }
}
