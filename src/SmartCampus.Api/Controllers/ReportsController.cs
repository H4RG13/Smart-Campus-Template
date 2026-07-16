using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.Application.Features.Reports;
using SmartCampus.Application.Features.Reports.Custom;
using SmartCampus.Domain.Constants;

namespace SmartCampus.Api.Controllers;

[Route("api/v1/reports")]
[Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Teacher}")]
public sealed class ReportsController(
    GetAttendanceReportUseCase getAttendanceReportUseCase,
    GetCustomReportUseCase getCustomReportUseCase) : ApiControllerBase
{
    [HttpGet("attendance")]
    public async Task<IActionResult> GetAttendanceReport(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] string format,
        CancellationToken cancellationToken)
    {
        var rows = await getAttendanceReportUseCase.ExecuteAsync(fromDate, toDate, cancellationToken);

        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = AttendanceReportCsvWriter.Write(rows);
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"attendance-report-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.csv");
        }

        return Ok(rows);
    }

    [HttpGet("custom/{reportKey}")]
    public async Task<IActionResult> GetCustomReport(string reportKey, CancellationToken cancellationToken)
    {
        var result = await getCustomReportUseCase.ExecuteAsync(reportKey, cancellationToken);
        return result is null ? NotFound() : File(result.Content, result.ContentType, result.FileName);
    }
}
