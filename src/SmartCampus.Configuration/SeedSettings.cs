namespace SmartCampus.Configuration;

public sealed class SeedSettings
{
    public const string SectionName = "Seed";

    public required string AdminEmail { get; init; }
    public required string AdminPassword { get; init; }
}
