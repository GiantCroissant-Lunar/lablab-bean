namespace LablabBean.Plugins.Spells.Data;

/// <summary>
/// Defines a spell with its properties and effects.
/// </summary>
public class Spell
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SpellType Type { get; set; }
    public TargetingType Targeting { get; set; }
    public int ManaCost { get; set; }
    public int Cooldown { get; set; }
    public int Range { get; set; }
    public int AreaRadius { get; set; }
    public int MinLevel { get; set; }
    public List<SpellEffect> Effects { get; set; } = new();
}

public enum SpellType
{
    Offensive,
    Defensive,
    Healing,
    Utility,
    AOE
}

public enum TargetingType
{
    Single,
    AOE,
    Self,
    Directional
}
