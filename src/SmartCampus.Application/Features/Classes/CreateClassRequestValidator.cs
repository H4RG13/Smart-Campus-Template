using FluentValidation;
using SmartCampus.Application.Common.Interfaces;

namespace SmartCampus.Application.Features.Classes;

public sealed class CreateClassRequestValidator : AbstractValidator<CreateClassRequest>
{
    public CreateClassRequestValidator(IAcademicTermRepository termRepository)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);

        RuleFor(x => x.AcademicTermId)
            .MustAsync(async (id, cancellationToken) => await termRepository.GetByIdAsync(id, cancellationToken) is not null)
            .WithMessage("Academic term does not exist.");
    }
}
