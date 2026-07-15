using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SmartCampus.Application.Features.Identity;

namespace SmartCampus.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
        services.AddScoped<LoginUseCase>();

        return services;
    }
}
