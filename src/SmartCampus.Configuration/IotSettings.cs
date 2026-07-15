namespace SmartCampus.Configuration;

public sealed class IotSettings
{
    public const string SectionName = "Iot";

    public required int HeartbeatIntervalSeconds { get; init; }
    public required int OfflineThresholdMultiplier { get; init; }
}
