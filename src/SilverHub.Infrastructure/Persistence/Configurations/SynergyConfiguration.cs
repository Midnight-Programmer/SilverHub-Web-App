using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SilverHub.Domain.Synergies;

namespace SilverHub.Infrastructure.Persistence.Configurations;

public sealed class SynergyConfiguration : IEntityTypeConfiguration<Synergy>
{
    public void Configure(EntityTypeBuilder<Synergy> builder)
    {
        builder.ToTable("synergies");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Slug).HasColumnName("slug").HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();

        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.IconKey).HasColumnName("icon_key").HasMaxLength(500);

        builder.Property(x => x.Icon2Key).HasColumnName("icon2_key").HasMaxLength(500);
        builder.Property(x => x.Icon3Key).HasColumnName("icon3_key").HasMaxLength(500);

        builder.Property(x => x.Description2Markdown).HasColumnName("description2_md");
        builder.Property(x => x.Description3Markdown).HasColumnName("description3_md");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasMany(x => x.Members).WithOne(x => x.Synergy).HasForeignKey(x => x.SynergyId);
    }
}
