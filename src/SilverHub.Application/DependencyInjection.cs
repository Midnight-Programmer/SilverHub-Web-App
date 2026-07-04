using Microsoft.Extensions.DependencyInjection;
using SilverHub.Application.Heroes.Queries;

namespace SilverHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<GetHeroesListHandler>();

        return services;
    }
}
