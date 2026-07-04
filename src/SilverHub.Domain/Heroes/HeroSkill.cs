namespace SilverHub.Domain.Heroes;

public enum SkillType
{
    Basic = 0,
    Ultimate = 1,
    Special = 2,
    Talent = 3
}

public sealed class HeroSkill
{
    private HeroSkill() { }

    public HeroSkill(
        Guid id,
        Guid heroId,
        SkillType type,
        string name,
        string? iconKey,
        string descriptionMarkdown,
        int? cost,
        string? valuesJson,
        string? buffsJson,
        string? debuffsJson,
        int sortOrder
    )
    {
        Id = id;
        HeroId = heroId;
        Type = type;
        Name = name;
        IconKey = iconKey;
        DescriptionMarkdown = descriptionMarkdown;
        Cost = cost;
        ValuesJson = valuesJson;
        BuffsJson = buffsJson;
        DebuffsJson = debuffsJson;
        SortOrder = sortOrder;
    }

    public Guid Id { get; private set; }
    public Guid HeroId { get; private set; }
    public SkillType Type { get; private set; }
    public string Name { get; private set; } = default!;
    public string? IconKey { get; private set; }
    public string DescriptionMarkdown { get; private set; } = default!;
    public int? Cost { get; private set; }

    public string? ValuesJson { get; private set; }
    public string? BuffsJson { get; private set; }
    public string? DebuffsJson { get; private set; }

    public int SortOrder { get; private set; }

    public Hero Hero { get; private set; } = default!;
}
