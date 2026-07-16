using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.Application.Features.Guardians;
using SmartCampus.Domain.Constants;

namespace SmartCampus.Api.Controllers;

[Route("api/v1/guardians")]
[Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Staff}")]
public sealed class GuardiansController(
    CreateGuardianUseCase createGuardianUseCase,
    GetGuardiansUseCase getGuardiansUseCase,
    LinkGuardianToStudentUseCase linkGuardianToStudentUseCase,
    IValidator<CreateGuardianRequest> createGuardianValidator,
    IValidator<LinkGuardianToStudentRequest> linkGuardianValidator) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GuardianDto>>> GetGuardians(CancellationToken cancellationToken)
    {
        return Ok(await getGuardiansUseCase.ExecuteAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<GuardianDto>> CreateGuardian(CreateGuardianRequest request, CancellationToken cancellationToken)
    {
        if (await ValidateAsync(createGuardianValidator, request, cancellationToken) is { } validationError)
        {
            return validationError;
        }

        var guardian = await createGuardianUseCase.ExecuteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetGuardians), new { }, guardian);
    }

    [HttpPost("{id:guid}/students")]
    public async Task<ActionResult<GuardianDto>> LinkToStudent(Guid id, LinkGuardianToStudentRequest request, CancellationToken cancellationToken)
    {
        if (await ValidateAsync(linkGuardianValidator, request, cancellationToken) is { } validationError)
        {
            return validationError;
        }

        try
        {
            var guardian = await linkGuardianToStudentUseCase.ExecuteAsync(id, request, cancellationToken);
            return guardian is null ? NotFound() : Ok(guardian);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: ex.Message);
        }
    }
}
