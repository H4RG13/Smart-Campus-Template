using FluentValidation;
using SmartCampus.Application.Common.Interfaces;

namespace SmartCampus.Application.Features.Guardians;

public sealed class LinkGuardianToStudentRequestValidator : AbstractValidator<LinkGuardianToStudentRequest>
{
    public LinkGuardianToStudentRequestValidator(IStudentRepository studentRepository)
    {
        RuleFor(x => x.Relationship).NotEmpty().MaximumLength(50);

        RuleFor(x => x.StudentId)
            .MustAsync(async (id, cancellationToken) => await studentRepository.GetByIdAsync(id, cancellationToken) is not null)
            .WithMessage("Student does not exist.");
    }
}
