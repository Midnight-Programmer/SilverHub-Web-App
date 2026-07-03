namespace SilverHub.Domain.Heroes;

public sealed class HeroTag
{
    private HeroTag() { }

    public HeroTag(Guid heroId, string tag)
    {
        HeroId = heroId;
        Tag = tag;
    }

    public Guid HeroId { get; private set; }
    public string Tag { get; private set; } = default!;

    public Hero Hero { get; private set; } = default!;
}
