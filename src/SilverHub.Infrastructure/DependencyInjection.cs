using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SilverHub.Application.Heroes.Ports;
using SilverHub.Infrastructure.Persistence;
using SilverHub.Infrastructure.Repositories;

namespace SilverHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<SilverHubDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("SilverHubDb")));

        services.AddScoped<IHeroRepository, HeroRepository>();

        return services;
    }
}
