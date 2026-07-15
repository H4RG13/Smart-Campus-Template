using FluentValidation;

namespace SmartCampus.Application.Features.AcademicCalendar;

public sealed class CreateTermRequestValidator : AbstractValidator<CreateTermRequest>
{
    public CreateTermRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date.");
    }
}
