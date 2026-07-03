using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SilverHub.Domain.Synergies;

namespace SilverHub.Infrastructure.Persistence.Configurations;

public sealed class SynergyMemberConfiguration : IEntityTypeConfiguration<SynergyMember>
{
    public void Configure(EntityTypeBuilder<SynergyMember> builder)
    {
        builder.ToTable("synergy_members");

        builder.HasKey(x => new { x.SynergyId, x.HeroId });
        builder.Property(x => x.SynergyId).HasColumnName("synergy_id");
        builder.Property(x => x.HeroId).HasColumnName("hero_id");

        builder.Property(x => x.Role).HasColumnName("role").HasConversion<string>().IsRequired();
        builder.Property(x => x.SortOrder).HasColumnName("sort_order").IsRequired();

        builder.HasIndex(x => new { x.SynergyId, x.Role, x.SortOrder });
        builder.HasIndex(x => x.HeroId);
    }
}
