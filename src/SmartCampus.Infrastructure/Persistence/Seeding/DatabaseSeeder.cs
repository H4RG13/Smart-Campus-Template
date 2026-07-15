using Microsoft.EntityFrameworkCore;
using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Configuration;
using SmartCampus.Domain.Constants;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Seeding;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        AppDbContext dbContext,
        IPasswordHasher passwordHasher,
        SeedSettings seedSettings,
        CancellationToken cancellationToken = default)
    {
        foreach (var roleName in RoleNames.All)
        {
            var exists = await dbContext.Roles.AnyAsync(r => r.Name == roleName, cancellationToken);
            if (!exists)
            {
                dbContext.Roles.Add(new Role(roleName));
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var adminExists = await dbContext.Users.AnyAsync(cancellationToken);
        if (!adminExists)
        {
            var adminRole = await dbContext.Roles.SingleAsync(r => r.Name == RoleNames.Admin, cancellationToken);
            var admin = new User(seedSettings.AdminEmail, passwordHasher.Hash(seedSettings.AdminPassword), DateTime.UtcNow);
            admin.AssignRole(adminRole);

            dbContext.Users.Add(admin);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
