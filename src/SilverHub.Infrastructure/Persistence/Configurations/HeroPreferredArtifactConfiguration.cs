using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SilverHub.Domain.Heroes;

namespace SilverHub.Infrastructure.Persistence.Configurations;

public sealed class HeroPreferredArtifactConfiguration : IEntityTypeConfiguration<HeroPreferredArtifact>
{
    public void Configure(EntityTypeBuilder<HeroPreferredArtifact> builder)
    {
        builder.ToTable("hero_preferred_artifacts");

        builder.HasKey(x => new { x.HeroId, x.ArtifactName });

        builder.Property(x => x.HeroId).HasColumnName("hero_id");
        builder.Property(x => x.ArtifactName).HasColumnName("artifact_name").HasMaxLength(200);
        builder.Property(x => x.ArtifactSlug).HasColumnName("artifact_slug").HasMaxLength(200);
        builder.Property(x => x.SortOrder).HasColumnName("sort_order").IsRequired();

        builder.HasIndex(x => new { x.HeroId, x.SortOrder });
    }
}
