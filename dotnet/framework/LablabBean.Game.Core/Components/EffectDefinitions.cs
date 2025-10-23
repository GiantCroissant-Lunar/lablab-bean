namespace LablabBean.Game.Core.Components;

/// <summary>
/// Predefined status effect definitions with default values.
/// </summary>
public static class EffectDefinitions
{
    // Damage Over Time Effects

    public static readonly EffectDefinition Poison = new()
    {
        Type = EffectType.Poison,
        DefaultMagnitude = 3,      // 3 HP damage per turn
        DefaultDuration = 5,        // 5 turns
        Category = EffectCategory.DamageOverTime,
        Color = EffectColor.Red,
        Description = "Deals 3 damage per turn for 5 turns"
    };

    public static readonly EffectDefinition Bleed = new()
    {
        Type = EffectType.Bleed,
        DefaultMagnitude = 2,      // 2 HP damage per turn
        DefaultDuration = 8,        // 8 turns
        Category = EffectCategory.DamageOverTime,
        Color = EffectColor.DarkRed,
        Description = "Deals 2 damage per turn for 8 turns"
    };

    public static readonly EffectDefinition Burning = new()
    {
        Type = EffectType.Burning,
        DefaultMagnitude = 4,      // 4 HP damage per turn
        DefaultDuration = 3,        // 3 turns
        Category = EffectCategory.DamageOverTime,
        Color = EffectColor.Orange,
        Description = "Deals 4 damage per turn for 3 turns"
    };

    // Healing Over Time Effects

    public static readonly EffectDefinition Regeneration = new()
    {
        Type = EffectType.Regeneration,
        DefaultMagnitude = 2,      // 2 HP healing per turn
        DefaultDuration = 10,       // 10 turns
        Category = EffectCategory.HealingOverTime,
        Color = EffectColor.Green,
        Description = "Heals 2 HP per turn for 10 turns"
    };

    public static readonly EffectDefinition Blessed = new()
    {
        Type = EffectType.Blessed,
        DefaultMagnitude = 1,      // 1 HP healing per turn
        DefaultDuration = 20,       // 20 turns
        Category = EffectCategory.HealingOverTime,
        Color = EffectColor.Yellow,
        Description = "Heals 1 HP per turn for 20 turns"
    };

    // Stat Buff Effects

    public static readonly EffectDefinition Strength = new()
    {
        Type = EffectType.Strength,
        DefaultMagnitude = 5,      // +5 attack
        DefaultDuration = 10,       // 10 turns
        Category = EffectCategory.StatBuff,
        Color = EffectColor.Green,
        Description = "Increases attack by 5 for 10 turns"
    };

    public static readonly EffectDefinition Haste = new()
    {
        Type = EffectType.Haste,
        DefaultMagnitude = 20,     // +20 speed
        DefaultDuration = 8,        // 8 turns
        Category = EffectCategory.StatBuff,
        Color = EffectColor.Cyan,
        Description = "Increases speed by 20 for 8 turns"
    };

    public static readonly EffectDefinition IronSkin = new()
    {
        Type = EffectType.IronSkin,
        DefaultMagnitude = 5,      // +5 defense
        DefaultDuration = 12,       // 12 turns
        Category = EffectCategory.StatBuff,
        Color = EffectColor.Blue,
        Description = "Increases defense by 5 for 12 turns"
    };

    // Stat Debuff Effects

    public static readonly EffectDefinition Weakness = new()
    {
        Type = EffectType.Weakness,
        DefaultMagnitude = 3,      // -3 attack
        DefaultDuration = 6,        // 6 turns
        Category = EffectCategory.StatDebuff,
        Color = EffectColor.Red,
        Description = "Decreases attack by 3 for 6 turns"
    };

    public static readonly EffectDefinition Slow = new()
    {
        Type = EffectType.Slow,
        DefaultMagnitude = 30,     // -30 speed
        DefaultDuration = 6,        // 6 turns
        Category = EffectCategory.StatDebuff,
        Color = EffectColor.Red,
        Description = "Decreases speed by 30 for 6 turns"
    };

    public static readonly EffectDefinition Fragile = new()
    {
        Type = EffectType.Fragile,
        DefaultMagnitude = 3,      // -3 defense
        DefaultDuration = 6,        // 6 turns
        Category = EffectCategory.StatDebuff,
        Color = EffectColor.Red,
        Description = "Decreases defense by 3 for 6 turns"
    };

    /// <summary>
    /// Get effect definition by type
    /// </summary>
    public static EffectDefinition GetDefinition(EffectType type)
    {
        return type switch
        {
            EffectType.Poison => Poison,
            EffectType.Bleed => Bleed,
            EffectType.Burning => Burning,
            EffectType.Regeneration => Regeneration,
            EffectType.Blessed => Blessed,
            EffectType.Strength => Strength,
            EffectType.Haste => Haste,
            EffectType.IronSkin => IronSkin,
            EffectType.Weakness => Weakness,
            EffectType.Slow => Slow,
            EffectType.Fragile => Fragile,
            _ => throw new ArgumentException($"Unknown effect type: {type}")
        };
    }
}
