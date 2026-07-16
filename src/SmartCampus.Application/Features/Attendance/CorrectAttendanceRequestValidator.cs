using FluentValidation;

namespace SmartCampus.Application.Features.Attendance;

public sealed class CorrectAttendanceRequestValidator : AbstractValidator<CorrectAttendanceRequest>
{
    public CorrectAttendanceRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
