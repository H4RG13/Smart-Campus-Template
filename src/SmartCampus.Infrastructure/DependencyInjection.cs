using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartCampus.Infrastructure.Persistence;

namespace SmartCampus.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>();

        return services;
    }
}
