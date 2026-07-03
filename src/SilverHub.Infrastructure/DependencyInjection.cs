using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SilverHub.Infrastructure.Persistence;

namespace SilverHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<SilverHubDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("SilverHubDb")));

        return services;
    }
}
