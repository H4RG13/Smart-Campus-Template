using SmartCampus.Domain.Enums;

namespace SmartCampus.Application.Features.Devices;

public sealed class SubmitDeviceEventResponse
{
    public required DeviceEventProcessingStatus ProcessingStatus { get; init; }
    public AttendanceStatus? AttendanceStatus { get; init; }
    public string? StudentDisplayName { get; init; }
}
