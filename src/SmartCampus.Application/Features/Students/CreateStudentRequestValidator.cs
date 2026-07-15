using FluentValidation;
using SmartCampus.Application.Common.Interfaces;

namespace SmartCampus.Application.Features.Students;

public sealed class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator(IStudentRepository studentRepository, IClassRepository classRepository)
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.StudentNumber)
            .NotEmpty()
            .MaximumLength(50)
            .MustAsync(async (studentNumber, cancellationToken) =>
                !await studentRepository.StudentNumberExistsAsync(studentNumber, cancellationToken))
            .WithMessage("Student number is already in use.");

        RuleFor(x => x.ClassId)
            .MustAsync(async (id, cancellationToken) => await classRepository.ExistsAsync(id, cancellationToken))
            .WithMessage("Class does not exist.");

        RuleFor(x => x.DateOfBirth)
            .LessThan(x => DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date of birth must be in the past.");
    }
}
