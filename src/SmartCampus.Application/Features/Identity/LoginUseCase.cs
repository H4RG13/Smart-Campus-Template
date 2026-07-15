using SmartCampus.Application.Common.Exceptions;
using SmartCampus.Application.Common.Interfaces;

namespace SmartCampus.Application.Features.Identity;

public sealed class LoginUseCase(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator)
{
    public async Task<LoginResponse> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || !user.IsActive || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new AuthenticationFailedException();
        }

        var token = jwtTokenGenerator.GenerateToken(user);

        return new LoginResponse
        {
            Token = token.Token,
            ExpiresAtUtc = token.ExpiresAtUtc,
            Email = user.Email,
            Roles = user.Roles.Select(r => r.Name).ToList(),
        };
    }
}
