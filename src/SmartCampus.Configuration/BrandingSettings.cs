namespace SmartCampus.Configuration;

public sealed class BrandingSettings
{
    public const string SectionName = "Branding";

    public required string SchoolName { get; init; }
    public required string LogoUrl { get; init; }
    public required string PrimaryColor { get; init; }
    public required string SecondaryColor { get; init; }
    public required string Timezone { get; init; }
}
