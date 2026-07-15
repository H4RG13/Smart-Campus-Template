using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.Application.Features.Staff;
using SmartCampus.Domain.Constants;

namespace SmartCampus.Api.Controllers;

[Route("api/v1/staff")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class StaffController(
    CreateStaffUseCase createStaffUseCase,
    GetStaffListUseCase getStaffListUseCase,
    IValidator<CreateStaffRequest> createStaffValidator) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StaffDto>>> GetStaff(CancellationToken cancellationToken)
    {
        return Ok(await getStaffListUseCase.ExecuteAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<StaffDto>> CreateStaff(CreateStaffRequest request, CancellationToken cancellationToken)
    {
        if (await ValidateAsync(createStaffValidator, request, cancellationToken) is { } validationError)
        {
            return validationError;
        }

        var staff = await createStaffUseCase.ExecuteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetStaff), new { }, staff);
    }
}
