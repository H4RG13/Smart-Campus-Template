using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.Application.Features.Students;
using SmartCampus.Domain.Constants;

namespace SmartCampus.Api.Controllers;

[Route("api/v1/students")]
[Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Teacher},{RoleNames.Staff}")]
public sealed class StudentsController(
    CreateStudentUseCase createStudentUseCase,
    GetStudentsUseCase getStudentsUseCase,
    GetStudentByIdUseCase getStudentByIdUseCase,
    AssignRfidTagUseCase assignRfidTagUseCase,
    IValidator<CreateStudentRequest> createStudentValidator,
    IValidator<AssignRfidTagRequest> assignRfidTagValidator) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StudentDto>>> GetStudents(CancellationToken cancellationToken)
    {
        return Ok(await getStudentsUseCase.ExecuteAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StudentDto>> GetStudent(Guid id, CancellationToken cancellationToken)
    {
        var student = await getStudentByIdUseCase.ExecuteAsync(id, cancellationToken);
        return student is null ? NotFound() : Ok(student);
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<StudentDto>> CreateStudent(CreateStudentRequest request, CancellationToken cancellationToken)
    {
        if (await ValidateAsync(createStudentValidator, request, cancellationToken) is { } validationError)
        {
            return validationError;
        }

        var student = await createStudentUseCase.ExecuteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, student);
    }

    [HttpPost("{id:guid}/rfid-tag")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Staff}")]
    public async Task<ActionResult<StudentDto>> AssignRfidTag(Guid id, AssignRfidTagRequest request, CancellationToken cancellationToken)
    {
        if (await ValidateAsync(assignRfidTagValidator, request, cancellationToken) is { } validationError)
        {
            return validationError;
        }

        var student = await assignRfidTagUseCase.ExecuteAsync(id, request, cancellationToken);
        return student is null ? NotFound() : Ok(student);
    }
}
