using Microsoft.EntityFrameworkCore;

namespace SilverHub.Infrastructure.Persistence;

public sealed class SilverHubDbContext : DbContext
{
    public SilverHubDbContext(DbContextOptions<SilverHubDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SilverHubDbContext).Assembly);
    }
}
