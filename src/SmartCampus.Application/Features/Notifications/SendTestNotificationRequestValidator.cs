using FluentValidation;

namespace SmartCampus.Application.Features.Notifications;

public sealed class SendTestNotificationRequestValidator : AbstractValidator<SendTestNotificationRequest>
{
    public SendTestNotificationRequestValidator()
    {
        RuleFor(x => x.ToAddress).NotEmpty().EmailAddress();
    }
}
