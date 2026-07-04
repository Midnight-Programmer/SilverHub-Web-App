using Microsoft.EntityFrameworkCore;
using SilverHub.Domain.Heroes;
using SilverHub.Domain.Synergies;

namespace SilverHub.Infrastructure.Persistence;

public sealed class SilverHubDbContext : DbContext
{
    public SilverHubDbContext(DbContextOptions<SilverHubDbContext> options) : base(options) { }

    public DbSet<Hero> Heroes => Set<Hero>();
    public DbSet<HeroImage> HeroImages => Set<HeroImage>();
    public DbSet<HeroSkill> HeroSkills => Set<HeroSkill>();
    public DbSet<HeroTag> HeroTags => Set<HeroTag>();
    public DbSet<HeroPreferredArtifact> HeroPreferredArtifacts => Set<HeroPreferredArtifact>();

    public DbSet<Synergy> Synergies => Set<Synergy>();
    public DbSet<SynergyMember> SynergyMembers => Set<SynergyMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SilverHubDbContext).Assembly);
    }
}
