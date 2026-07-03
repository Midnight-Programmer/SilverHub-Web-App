namespace SilverHub.Domain.Heroes;

public enum HeroImageKind
{
    Portrait = 0,
    Avatar = 1,
    Skin = 2,
    SkillIcon = 3
}

public sealed class HeroImage
{
    private HeroImage() { }

    public HeroImage(Guid id, Guid heroId, HeroImageKind kind, string imageKey, string? variantName, int sortOrder)
    {
        Id = id;
        HeroId = heroId;
        Kind = kind;
        ImageKey = imageKey;
        VariantName = variantName;
        SortOrder = sortOrder;
    }

    public Guid Id { get; private set; }
    public Guid HeroId { get; private set; }
    public HeroImageKind Kind { get; private set; }
    public string ImageKey { get; private set; } = default!;
    public string? VariantName { get; private set; }
    public int SortOrder { get; private set; }

    public Hero Hero { get; private set; } = default!;
}
