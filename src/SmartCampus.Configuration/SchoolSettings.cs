namespace SmartCampus.Configuration;

public sealed class SchoolSettings
{
    public const string SectionName = "School";

    public required int AttendanceLateThresholdMinutes { get; init; }
    public required string DefaultTimezone { get; init; }
    public required TimeOnly SchoolDayStartTime { get; init; }
}
