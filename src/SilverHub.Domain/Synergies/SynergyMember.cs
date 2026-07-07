using SilverHub.Domain.Heroes;

namespace SilverHub.Domain.Synergies;

public enum SynergyRole
{
    Oathsworn = 0,
    Initiate = 1
}

public sealed class SynergyMember
{
    private SynergyMember() { }

    public SynergyMember(Guid synergyId, Guid heroId, SynergyRole role, int sortOrder)
    {
        SynergyId = synergyId;
        HeroId = heroId;
        Role = role;
        SortOrder = sortOrder;
    }

    public Guid SynergyId { get; private set; }
    public Guid HeroId { get; private set; }
    public SynergyRole Role { get; private set; }
    public int SortOrder { get; private set; }

    public Synergy Synergy { get; private set; } = default!;
    public Hero Hero { get; private set; } = default!;
}
