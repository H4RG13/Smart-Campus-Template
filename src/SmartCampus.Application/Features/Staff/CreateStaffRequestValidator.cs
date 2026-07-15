using FluentValidation;
using SmartCampus.Application.Common.Interfaces;

namespace SmartCampus.Application.Features.Staff;

public sealed class CreateStaffRequestValidator : AbstractValidator<CreateStaffRequest>
{
    public CreateStaffRequestValidator(IStaffRepository staffRepository)
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Position).NotEmpty().MaximumLength(100);

        RuleFor(x => x.EmployeeNumber)
            .NotEmpty()
            .MaximumLength(50)
            .MustAsync(async (employeeNumber, cancellationToken) =>
                !await staffRepository.EmployeeNumberExistsAsync(employeeNumber, cancellationToken))
            .WithMessage("Employee number is already in use.");
    }
}
