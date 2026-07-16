namespace SmartCampus.Application.Features.Reports.Custom;

/// <summary>
/// The extension point for a school's bespoke report — see this folder's README.md
/// and docs/ARCHITECTURE.md §9/RULES.md #9. V1 ships zero handlers registered; the
/// first real custom report a client asks for is added here, in its own file,
/// without touching any shared reporting code.
/// </summary>
public interface ICustomReportHandler
{
    /// <summary>Matches the {reportKey} route segment in GET /reports/custom/{reportKey}.</summary>
    string ReportKey { get; }

    Task<CustomReportResult> GenerateAsync(CancellationToken cancellationToken = default);
}

public sealed class CustomReportResult
{
    public required string ContentType { get; init; }
    public required byte[] Content { get; init; }
    public required string FileName { get; init; }
}
