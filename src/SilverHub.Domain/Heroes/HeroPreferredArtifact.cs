namespace SilverHub.Domain.Heroes;

public sealed class HeroPreferredArtifact
{
    private HeroPreferredArtifact() { }

    public HeroPreferredArtifact(Guid heroId, string artifactName, int sortOrder)
    {
        HeroId = heroId;
        ArtifactName = artifactName;
        SortOrder = sortOrder;
    }

    public HeroPreferredArtifact(Guid heroId, string artifactName, string? artifactSlug, int sortOrder)
    {
        HeroId = heroId;
        ArtifactName = artifactName;
        ArtifactSlug = artifactSlug;
        SortOrder = sortOrder;
    }

    public Guid HeroId { get; private set; }
    public string ArtifactName { get; private set; } = default!;
    public string? ArtifactSlug { get; private set; }
    public int SortOrder { get; private set; }

    public Hero Hero { get; private set; } = default!;
}
