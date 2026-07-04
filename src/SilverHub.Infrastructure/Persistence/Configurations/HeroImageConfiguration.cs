using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SilverHub.Domain.Heroes;

namespace SilverHub.Infrastructure.Persistence.Configurations;

public sealed class HeroImageConfiguration : IEntityTypeConfiguration<HeroImage>
{
    public void Configure(EntityTypeBuilder<HeroImage> builder)
    {
        builder.ToTable("hero_images");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.HeroId).HasColumnName("hero_id").IsRequired();

        builder.Property(x => x.Kind).HasColumnName("kind").HasConversion<string>().IsRequired();
        builder.Property(x => x.ImageKey).HasColumnName("image_key").HasMaxLength(500).IsRequired();
        builder.Property(x => x.VariantName).HasColumnName("variant_name").HasMaxLength(200);

        builder.Property(x => x.SortOrder).HasColumnName("sort_order").IsRequired();

        builder.HasIndex(x => new { x.HeroId, x.Kind, x.SortOrder });
    }
}
