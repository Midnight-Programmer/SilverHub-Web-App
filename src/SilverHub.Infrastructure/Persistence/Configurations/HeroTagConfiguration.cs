using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SilverHub.Domain.Heroes;

namespace SilverHub.Infrastructure.Persistence.Configurations;

public sealed class HeroTagConfiguration : IEntityTypeConfiguration<HeroTag>
{
    public void Configure(EntityTypeBuilder<HeroTag> builder)
    {
        builder.ToTable("hero_tags");

        builder.HasKey(x => new { x.HeroId, x.Tag });

        builder.Property(x => x.HeroId).HasColumnName("hero_id");
        builder.Property(x => x.Tag).HasColumnName("tag").HasMaxLength(100);

        builder.HasIndex(x => x.Tag);
    }
}
