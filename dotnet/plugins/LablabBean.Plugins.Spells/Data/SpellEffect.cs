namespace LablabBean.Plugins.Spells.Data;

/// <summary>
/// Defines an effect that a spell applies.
/// </summary>
public class SpellEffect
{
    public SpellEffectType Type { get; set; }
    public int Value { get; set; }
    public int Duration { get; set; }
    public string? StatusEffectId { get; set; }
}

public enum SpellEffectType
{
    Damage,
    Heal,
    Shield,
    Buff,
    Debuff,
    StatusEffect,
    Teleport,
    Reveal
}
