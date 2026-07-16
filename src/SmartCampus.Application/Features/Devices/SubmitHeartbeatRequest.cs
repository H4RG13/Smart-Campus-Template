namespace SmartCampus.Application.Features.Devices;

public sealed class SubmitHeartbeatRequest
{
    public required string FirmwareVersion { get; init; }
    public required int SignalStrength { get; init; }
    public required int QueueDepth { get; init; }
}
