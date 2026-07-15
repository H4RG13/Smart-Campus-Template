using SmartCampus.Domain.Entities;

namespace SmartCampus.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    GeneratedToken GenerateToken(User user);
}

public sealed record GeneratedToken(string Token, DateTime ExpiresAtUtc);
