namespace LablabBean.Plugins.Spell.Data;

/// <summary>
/// Defines a spell that can be learned and cast by entities.
/// </summary>
public class Spell
{
    public string SpellId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int ManaCost { get; set; }
    public int Cooldown { get; set; }
    public int BaseDamage { get; set; }

    public SpellType SpellType { get; set; }
    public TargetType TargetType { get; set; }

    public int RequiredLevel { get; set; } = 1;

    public List<SpellEffectData> Effects { get; set; } = new();
}

/// <summary>
/// Type of spell effect.
/// </summary>
public enum SpellType
{
    Offensive,
    Defensive,
    Healing,
    Buff,
    Debuff
}

/// <summary>
/// Targeting type for spell.
/// </summary>
public enum TargetType
{
    Self,
    Single,
    Area
}

/// <summary>
/// Data for spell effects (buffs, debuffs, DoT).
/// </summary>
public class SpellEffectData
{
    public EffectType EffectType { get; set; }
    public int Duration { get; set; }
    public int Intensity { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Type of effect applied by spell.
/// </summary>
public enum EffectType
{
    None,
    Burn,
    Freeze,
    Slow,
    Stun,
    Shield,
    Heal,
    HealthRegen,
    AttackBoost,
    DefenseBoost,
    SpeedBoost
}
