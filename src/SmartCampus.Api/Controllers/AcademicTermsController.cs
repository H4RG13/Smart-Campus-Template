using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.Application.Features.AcademicCalendar;
using SmartCampus.Domain.Constants;

namespace SmartCampus.Api.Controllers;

[Route("api/v1/academic-terms")]
[Authorize]
public sealed class AcademicTermsController(
    CreateTermUseCase createTermUseCase,
    GetTermsUseCase getTermsUseCase,
    AddCalendarExceptionUseCase addCalendarExceptionUseCase,
    IValidator<CreateTermRequest> createTermValidator,
    IValidator<AddCalendarExceptionRequest> addExceptionValidator) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TermDto>>> GetTerms(CancellationToken cancellationToken)
    {
        return Ok(await getTermsUseCase.ExecuteAsync(cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<TermDto>> CreateTerm(CreateTermRequest request, CancellationToken cancellationToken)
    {
        if (await ValidateAsync(createTermValidator, request, cancellationToken) is { } validationError)
        {
            return validationError;
        }

        var term = await createTermUseCase.ExecuteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetTerms), new { }, term);
    }

    [HttpPost("{id:guid}/exceptions")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<CalendarExceptionDto>> AddException(
        Guid id,
        [FromBody] AddCalendarExceptionRequestBody body,
        CancellationToken cancellationToken)
    {
        var request = new AddCalendarExceptionRequest
        {
            AcademicTermId = id,
            Date = body.Date,
            Type = body.Type,
            Description = body.Description,
        };

        if (await ValidateAsync(addExceptionValidator, request, cancellationToken) is { } validationError)
        {
            return validationError;
        }

        try
        {
            var exception = await addCalendarExceptionUseCase.ExecuteAsync(request, cancellationToken);
            return Ok(exception);
        }
        catch (ArgumentException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: ex.Message);
        }
    }
}

public sealed class AddCalendarExceptionRequestBody
{
    public required DateOnly Date { get; init; }
    public required Domain.Enums.CalendarExceptionType Type { get; init; }
    public string? Description { get; init; }
}
