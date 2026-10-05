using EkubCircle.Application.Interfaces;
using EkubCircle.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EkubCircle.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICircleService, CircleService>();
        services.AddScoped<IRoundService, RoundService>();
        services.AddScoped<IDashboardService, DashboardService>();
        return services;
    }
}
