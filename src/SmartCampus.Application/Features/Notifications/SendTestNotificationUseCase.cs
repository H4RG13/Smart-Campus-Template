using Microsoft.Extensions.Options;
using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Configuration;

namespace SmartCampus.Application.Features.Notifications;

/// <summary>Lets an admin verify SMTP config right after deployment — see docs/API_SPECIFICATION.md.</summary>
public sealed class SendTestNotificationUseCase(IEmailSender emailSender, IOptions<BrandingSettings> brandingSettings)
{
    public Task ExecuteAsync(SendTestNotificationRequest request, CancellationToken cancellationToken = default) =>
        emailSender.SendAsync(
            request.ToAddress,
            $"{brandingSettings.Value.SchoolName}: SmartCampus test email",
            "If you received this, SmartCampus's SMTP configuration is working correctly.",
            cancellationToken);
}
