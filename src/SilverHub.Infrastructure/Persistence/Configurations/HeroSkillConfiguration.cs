using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SilverHub.Domain.Heroes;

namespace SilverHub.Infrastructure.Persistence.Configurations;

public sealed class HeroSkillConfiguration : IEntityTypeConfiguration<HeroSkill>
{
    public void Configure(EntityTypeBuilder<HeroSkill> builder)
    {
        builder.ToTable("hero_skills");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.HeroId).HasColumnName("hero_id").IsRequired();

        builder.Property(x => x.Type).HasColumnName("skill_type").HasConversion<string>().IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.IconKey).HasColumnName("icon_key").HasMaxLength(500);
        builder.Property(x => x.DescriptionMarkdown).HasColumnName("description_md").IsRequired();
        builder.Property(x => x.Cost).HasColumnName("cost");
        builder.Property(x => x.ValuesJson).HasColumnName("values").HasColumnType("jsonb");
        builder.Property(x => x.BuffsJson).HasColumnName("buffs").HasColumnType("jsonb");
        builder.Property(x => x.DebuffsJson).HasColumnName("debuffs").HasColumnType("jsonb");

        builder.Property(x => x.SortOrder).HasColumnName("sort_order").IsRequired();

        builder.HasIndex(x => new { x.HeroId, x.Type, x.SortOrder });
    }
}
