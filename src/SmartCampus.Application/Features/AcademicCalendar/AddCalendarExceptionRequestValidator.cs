using FluentValidation;
using SmartCampus.Application.Common.Interfaces;

namespace SmartCampus.Application.Features.AcademicCalendar;

public sealed class AddCalendarExceptionRequestValidator : AbstractValidator<AddCalendarExceptionRequest>
{
    public AddCalendarExceptionRequestValidator(IAcademicTermRepository termRepository)
    {
        RuleFor(x => x.AcademicTermId)
            .MustAsync(async (id, cancellationToken) => await termRepository.GetByIdAsync(id, cancellationToken) is not null)
            .WithMessage("Academic term does not exist.");
    }
}
