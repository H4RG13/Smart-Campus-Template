namespace SmartCampus.Application.Features.Notifications;

public sealed class SendTestNotificationRequest
{
    public required string ToAddress { get; init; }
}
