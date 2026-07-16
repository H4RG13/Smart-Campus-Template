using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.Application.Features.Attendance;
using SmartCampus.Domain.Constants;

namespace SmartCampus.Api.Controllers;

[Route("api/v1/attendance-records")]
[Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Teacher},{RoleNames.Staff}")]
public sealed class AttendanceRecordsController(
    RecordAttendanceUseCase recordAttendanceUseCase,
    CorrectAttendanceUseCase correctAttendanceUseCase,
    GetAttendanceRecordsUseCase getAttendanceRecordsUseCase,
    GetAttendanceSummaryUseCase getAttendanceSummaryUseCase,
    IValidator<RecordAttendanceRequest> recordAttendanceValidator,
    IValidator<CorrectAttendanceRequest> correctAttendanceValidator) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AttendanceRecordDto>>> GetRecords(
        [FromQuery] Guid? studentId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        return Ok(await getAttendanceRecordsUseCase.ExecuteAsync(studentId, fromDate, toDate, cancellationToken));
    }

    [HttpGet("summary")]
    public async Task<ActionResult<IReadOnlyList<AttendanceDaySummaryDto>>> GetSummary(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        CancellationToken cancellationToken)
    {
        return Ok(await getAttendanceSummaryUseCase.ExecuteAsync(fromDate, toDate, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Teacher}")]
    public async Task<ActionResult<AttendanceRecordDto>> RecordAttendance(
        RecordAttendanceRequest request,
        CancellationToken cancellationToken)
    {
        if (await ValidateAsync(recordAttendanceValidator, request, cancellationToken) is { } validationError)
        {
            return validationError;
        }

        try
        {
            var record = await recordAttendanceUseCase.ExecuteAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetRecords), new { }, record);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<AttendanceRecordDto>> CorrectAttendance(
        Guid id,
        CorrectAttendanceRequest request,
        CancellationToken cancellationToken)
    {
        if (await ValidateAsync(correctAttendanceValidator, request, cancellationToken) is { } validationError)
        {
            return validationError;
        }

        var record = await correctAttendanceUseCase.ExecuteAsync(id, request, cancellationToken);
        return record is null ? NotFound() : Ok(record);
    }
}
