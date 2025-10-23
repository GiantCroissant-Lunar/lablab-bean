namespace LablabBean.Game.Core.Components;

/// <summary>
/// Represents a single active status effect on an entity.
/// Value type (struct) for performance.
/// </summary>
public struct StatusEffect
{
    /// <summary>Type of effect (Poison, Strength, Haste, etc.)</summary>
    public EffectType Type { get; set; }

    /// <summary>Effect magnitude (damage per turn, stat bonus, etc.)</summary>
    public int Magnitude { get; set; }

    /// <summary>Remaining duration in turns (1-99)</summary>
    public int Duration { get; set; }

    /// <summary>Category for grouping and processing</summary>
    public EffectCategory Category { get; set; }

    /// <summary>Source of effect (for tracking/debugging)</summary>
    public EffectSource Source { get; set; }

    /// <summary>Display color for HUD</summary>
    public EffectColor Color { get; set; }

    /// <summary>Display name for HUD</summary>
    public string DisplayName => Type.ToString();

    /// <summary>Whether this effect has expired</summary>
    public bool IsExpired => Duration <= 0;
}

/// <summary>
/// Component attached to entities (player, enemies) to track active status effects.
/// </summary>
public struct StatusEffects
{
    /// <summary>List of currently active effects</summary>
    public List<StatusEffect> ActiveEffects { get; set; }

    /// <summary>Maximum number of concurrent effects (10 for MVP)</summary>
    public int MaxEffects { get; set; }

    /// <summary>Current number of active effects</summary>
    public int Count => ActiveEffects?.Count ?? 0;

    /// <summary>Whether max effects reached</summary>
    public bool IsFull => Count >= MaxEffects;

    /// <summary>Initialize with empty effects list</summary>
    public static StatusEffects CreateEmpty()
    {
        return new StatusEffects
        {
            ActiveEffects = new List<StatusEffect>(),
            MaxEffects = 10
        };
    }
}

/// <summary>
/// Template for creating status effects.
/// Defines default values for each effect type.
/// </summary>
public struct EffectDefinition
{
    public EffectType Type { get; set; }
    public int DefaultMagnitude { get; set; }
    public int DefaultDuration { get; set; }
    public EffectCategory Category { get; set; }
    public EffectColor Color { get; set; }
    public string Description { get; set; }
}

public enum EffectType
{
    // Damage Over Time
    Poison,
    Bleed,
    Burning,

    // Healing Over Time
    Regeneration,
    Blessed,

    // Stat Buffs
    Strength,      // +Attack
    Haste,         // +Speed
    IronSkin,      // +Defense

    // Stat Debuffs
    Weakness,      // -Attack
    Slow,          // -Speed
    Fragile        // -Defense
}

public enum EffectCategory
{
    DamageOverTime,
    HealingOverTime,
    StatBuff,
    StatDebuff
}

public enum EffectSource
{
    Consumable,
    EnemyAttack,
    EnemySpell,
    Environmental,
    Other
}

public enum EffectColor
{
    Red,           // Debuffs, DoT
    Green,         // Buffs, HoT
    Blue,          // Defense buffs
    Cyan,          // Speed buffs
    Yellow,        // Healing
    Purple,        // Neutral
    Orange,        // Fire effects
    DarkRed        // Severe debuffs
}
