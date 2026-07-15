namespace SmartCampus.Application.Features.Identity;

public sealed class LoginResponse
{
    public required string Token { get; init; }
    public required DateTime ExpiresAtUtc { get; init; }
    public required string Email { get; init; }
    public required IReadOnlyList<string> Roles { get; init; }
}
