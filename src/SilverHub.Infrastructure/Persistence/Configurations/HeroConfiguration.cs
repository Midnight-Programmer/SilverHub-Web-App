using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SilverHub.Domain.Heroes;

namespace SilverHub.Infrastructure.Persistence.Configurations;

public sealed class HeroConfiguration : IEntityTypeConfiguration<Hero>
{
    public void Configure(EntityTypeBuilder<Hero> builder)
    {
        builder.ToTable("heroes");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Slug).HasColumnName("slug").HasMaxLength(200).IsRequired();

        builder.HasIndex(x => x.Slug).IsUnique();

        builder.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(200).IsRequired();

        builder.Property(x => x.CanonicalName).HasColumnName("canonical_name").HasMaxLength(200).IsRequired();

        builder.Property(x => x.Rarity).HasColumnName("rarity").HasConversion<string>().IsRequired();
        builder.Property(x => x.Faction).HasColumnName("faction").HasConversion<string>().IsRequired();
        builder.Property(x => x.EquipType).HasColumnName("equip_type").HasConversion<string>().IsRequired();
        builder.Property(x => x.Class).HasColumnName("class").HasConversion<string>().IsRequired();
        builder.Property(x => x.MoonType).HasColumnName("moon_type").HasConversion<string>().IsRequired();
        builder.Property(x => x.DamageType).HasColumnName("damage_type").HasConversion<string>().IsRequired();

        builder.Property(x => x.Boudoir).HasColumnName("boudoir").IsRequired();
        builder.Property(x => x.Limited).HasColumnName("limited").IsRequired();

        builder.Property(x => x.FriendshipMax).HasColumnName("friendship_max").IsRequired();

        builder.Property(x => x.ReleaseDate).HasColumnName("release_date");
        builder.Property(x => x.BreakdownMarkdown).HasColumnName("breakdown_markdown");

        builder.Property(x => x.HasResonantia).HasColumnName("has_resonantia").IsRequired();
        builder.Property(x => x.ResonantiaTraitsJson).HasColumnName("resonantia_traits_json").HasColumnType("jsonb");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasMany(x => x.Images).WithOne(x => x.Hero).HasForeignKey(x => x.HeroId);
        builder.HasMany(x => x.Skills).WithOne(x => x.Hero).HasForeignKey(x => x.HeroId);
        builder.HasMany(x => x.Tags).WithOne(x => x.Hero).HasForeignKey(x => x.HeroId);
        builder.HasMany(x => x.PreferredArtifacts).WithOne(x => x.Hero).HasForeignKey(x => x.HeroId);
        builder.HasMany(x => x.SynergyMemberships).WithOne(x => x.Hero).HasForeignKey(x => x.HeroId);
    }
}
