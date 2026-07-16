using FluentValidation;
using SmartCampus.Application.Common.Interfaces;

namespace SmartCampus.Application.Features.Students;

public sealed class AssignRfidTagRequestValidator : AbstractValidator<AssignRfidTagRequest>
{
    public AssignRfidTagRequestValidator(IStudentRepository studentRepository)
    {
        RuleFor(x => x.RfidTagId)
            .NotEmpty()
            .MaximumLength(100)
            .MustAsync(async (tagId, cancellationToken) => await studentRepository.GetByRfidTagAsync(tagId, cancellationToken) is null)
            .WithMessage("This RFID tag is already assigned to another student.");
    }
}
