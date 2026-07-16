using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartCampus.Application.Common.Exceptions;
using SmartCampus.Application.Features.Identity;
using SmartCampus.Domain.Constants;

namespace SmartCampus.Api.Controllers;

[Route("api/v1/auth")]
public sealed class AuthController(
    LoginUseCase loginUseCase,
    IValidator<LoginRequest> loginRequestValidator) : ApiControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        if (await ValidateAsync(loginRequestValidator, request, cancellationToken) is { } validationError)
        {
            return validationError;
        }

        try
        {
            var response = await loginUseCase.ExecuteAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (AuthenticationFailedException ex)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: ex.Message);
        }
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult<object> Me()
    {
        return Ok(new
        {
            Email = User.FindFirstValue(ClaimTypes.Email),
            Roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value),
        });
    }

    [HttpGet("admin-only")]
    [Authorize(Roles = RoleNames.Admin)]
    public ActionResult<object> AdminOnly()
    {
        return Ok(new { Message = "You have Admin access." });
    }
}
