using FluentValidation;
using SmartCampus.Application.Common.Interfaces;

namespace SmartCampus.Application.Features.Devices;

public sealed class RegisterDeviceRequestValidator : AbstractValidator<RegisterDeviceRequest>
{
    public RegisterDeviceRequestValidator(IDeviceRepository deviceRepository)
    {
        RuleFor(x => x.DeviceName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.HardwareId)
            .NotEmpty()
            .MaximumLength(100)
            .MustAsync(async (hardwareId, cancellationToken) =>
                !await deviceRepository.HardwareIdExistsAsync(hardwareId, cancellationToken))
            .WithMessage("A device with this hardware id is already registered.");
    }
}
