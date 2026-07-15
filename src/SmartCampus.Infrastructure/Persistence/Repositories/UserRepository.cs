using Microsoft.EntityFrameworkCore;
using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return dbContext.Users
            .Include(u => u.Roles)
            .SingleOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
    }
}
