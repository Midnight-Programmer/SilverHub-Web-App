namespace SilverHub.Domain.Synergies;

public sealed class Synergy
{
    private readonly List<SynergyMember> _members = new();

    private Synergy() { }

    public Synergy(
        Guid id,
        string slug,
        string name,
        string? iconKey,
        string? icon2Key,
        string? description2Markdown,
        string? icon3Key,
        string? description3Markdown
    )
    {
        Id = id;
        Slug = slug;
        Name = name;
        IconKey = iconKey;
        Icon2Key = icon2Key;
        Description2Markdown = description2Markdown;
        Icon3Key = icon3Key;
        Description3Markdown = description3Markdown;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Slug { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? IconKey { get; private set; }

    public string? Icon2Key { get; private set; }
    public string? Description2Markdown { get; private set; }

    public string? Icon3Key { get; private set; }
    public string? Description3Markdown { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<SynergyMember> Members => _members;

    public void UpdateSeedData(
        string slug,
        string name,
        string? iconKey,
        string? icon2Key,
        string? description2Markdown,
        string? icon3Key,
        string? description3Markdown
    )
    {
        Slug = slug;
        Name = name;
        IconKey = iconKey;
        Icon2Key = icon2Key;
        Description2Markdown = description2Markdown;
        Icon3Key = icon3Key;
        Description3Markdown = description3Markdown;
        Touch();
    }

    public void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
