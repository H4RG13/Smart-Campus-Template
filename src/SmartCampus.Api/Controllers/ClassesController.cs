using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.Application.Features.Classes;
using SmartCampus.Domain.Constants;

namespace SmartCampus.Api.Controllers;

[Route("api/v1/classes")]
[Authorize]
public sealed class ClassesController(
    CreateClassUseCase createClassUseCase,
    GetClassesUseCase getClassesUseCase,
    IValidator<CreateClassRequest> createClassValidator) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClassDto>>> GetClasses(CancellationToken cancellationToken)
    {
        return Ok(await getClassesUseCase.ExecuteAsync(cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<ClassDto>> CreateClass(CreateClassRequest request, CancellationToken cancellationToken)
    {
        if (await ValidateAsync(createClassValidator, request, cancellationToken) is { } validationError)
        {
            return validationError;
        }

        var schoolClass = await createClassUseCase.ExecuteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetClasses), new { }, schoolClass);
    }
}
