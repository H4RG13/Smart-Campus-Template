using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using SmartCampus.Application.Common.Exceptions;
using SmartCampus.Application.Features.Devices;
using SmartCampus.Domain.Constants;

namespace SmartCampus.Api.Controllers;

[Route("api/v1/devices")]
public sealed class DevicesController(
    RegisterDeviceUseCase registerDeviceUseCase,
    GetDevicesUseCase getDevicesUseCase,
    GetDeviceEventsUseCase getDeviceEventsUseCase,
    SubmitDeviceEventUseCase submitDeviceEventUseCase,
    SubmitHeartbeatUseCase submitHeartbeatUseCase,
    IValidator<RegisterDeviceRequest> registerDeviceValidator) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Staff}")]
    public async Task<ActionResult<IReadOnlyList<DeviceDto>>> GetDevices(CancellationToken cancellationToken)
    {
        return Ok(await getDevicesUseCase.ExecuteAsync(cancellationToken));
    }

    [HttpGet("{deviceId:guid}/events")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<IReadOnlyList<DeviceEventDto>>> GetDeviceEvents(Guid deviceId, CancellationToken cancellationToken)
    {
        return Ok(await getDeviceEventsUseCase.ExecuteAsync(deviceId, cancellationToken));
    }

    [HttpPost("register")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<RegisterDeviceResponse>> Register(RegisterDeviceRequest request, CancellationToken cancellationToken)
    {
        if (await ValidateAsync(registerDeviceValidator, request, cancellationToken) is { } validationError)
        {
            return validationError;
        }

        var response = await registerDeviceUseCase.ExecuteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetDevices), new { }, response);
    }

    [HttpPost("{deviceId:guid}/events")]
    [AllowAnonymous]
    public async Task<ActionResult<SubmitDeviceEventResponse>> SubmitEvent(
        Guid deviceId,
        SubmitDeviceEventRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetDeviceToken(out var token))
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Missing device token.");
        }

        try
        {
            var response = await submitDeviceEventUseCase.ExecuteAsync(deviceId, token, request, cancellationToken);
            return Ok(response);
        }
        catch (DeviceAuthenticationFailedException ex)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: ex.Message);
        }
    }

    [HttpPost("{deviceId:guid}/heartbeat")]
    [AllowAnonymous]
    public async Task<IActionResult> Heartbeat(Guid deviceId, SubmitHeartbeatRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetDeviceToken(out var token))
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Missing device token.");
        }

        try
        {
            await submitHeartbeatUseCase.ExecuteAsync(deviceId, token, request, cancellationToken);
            return Ok();
        }
        catch (DeviceAuthenticationFailedException ex)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: ex.Message);
        }
    }

    private bool TryGetDeviceToken(out string token)
    {
        token = string.Empty;

        if (!Request.Headers.TryGetValue(HeaderNames.Authorization, out var value))
        {
            return false;
        }

        var header = value.ToString();
        const string prefix = "Bearer ";
        if (!header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        token = header[prefix.Length..];
        return !string.IsNullOrWhiteSpace(token);
    }
}
