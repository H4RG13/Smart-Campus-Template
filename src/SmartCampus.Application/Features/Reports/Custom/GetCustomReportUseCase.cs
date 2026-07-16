namespace SmartCampus.Application.Features.Reports.Custom;

public sealed class GetCustomReportUseCase(IEnumerable<ICustomReportHandler> handlers)
{
    public async Task<CustomReportResult?> ExecuteAsync(string reportKey, CancellationToken cancellationToken = default)
    {
        var handler = handlers.FirstOrDefault(h => h.ReportKey == reportKey);
        return handler is null ? null : await handler.GenerateAsync(cancellationToken);
    }
}
