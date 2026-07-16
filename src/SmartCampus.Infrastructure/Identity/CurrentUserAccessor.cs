using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SmartCampus.Application.Common.Interfaces;

namespace SmartCampus.Infrastructure.Identity;

public sealed class CurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    public Guid? UserId
    {
        get
        {
            var claims = httpContextAccessor.HttpContext?.User.Claims;
            var subjectClaim = claims?.FirstOrDefault(c => c.Type is ClaimTypes.NameIdentifier or "sub")?.Value;

            return Guid.TryParse(subjectClaim, out var userId) ? userId : null;
        }
    }
}
