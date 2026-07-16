using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.Application.Features.Notifications;
using SmartCampus.Domain.Constants;

namespace SmartCampus.Api.Controllers;

[Route("api/v1/notifications")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class NotificationsController(
    GetNotificationLogsUseCase getNotificationLogsUseCase,
    SendTestNotificationUseCase sendTestNotificationUseCase,
    IValidator<SendTestNotificationRequest> sendTestNotificationValidator) : ApiControllerBase
{
    [HttpGet("logs")]
    public async Task<ActionResult<IReadOnlyList<NotificationLogDto>>> GetLogs(CancellationToken cancellationToken)
    {
        return Ok(await getNotificationLogsUseCase.ExecuteAsync(cancellationToken));
    }

    [HttpPost("test")]
    public async Task<IActionResult> SendTest(SendTestNotificationRequest request, CancellationToken cancellationToken)
    {
        if (await ValidateAsync(sendTestNotificationValidator, request, cancellationToken) is { } validationError)
        {
            return validationError;
        }

        try
        {
            await sendTestNotificationUseCase.ExecuteAsync(request, cancellationToken);
            return Ok();
        }
        catch (Exception ex)
        {
            return Problem(statusCode: StatusCodes.Status502BadGateway, title: $"SMTP send failed: {ex.Message}");
        }
    }
}
