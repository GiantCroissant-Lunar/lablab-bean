using LablabBean.Plugins.Spell.Data;

namespace LablabBean.Plugins.Spell.Components;

/// <summary>
/// Component for active spell effects on an entity.
/// </summary>
public class SpellEffect
{
    public EffectType EffectType { get; set; }
    public int Duration { get; set; }
    public int Intensity { get; set; }
    public string SourceSpellId { get; set; } = string.Empty;

    public SpellEffect() { }

    public SpellEffect(EffectType effectType, int duration, int intensity, string sourceSpellId = "")
    {
        EffectType = effectType;
        Duration = duration;
        Intensity = intensity;
        SourceSpellId = sourceSpellId;
    }

    public bool IsExpired() => Duration <= 0;

    public void ReduceDuration(int amount = 1)
    {
        Duration = Math.Max(0, Duration - amount);
    }
}

/// <summary>
/// Collection of active effects on an entity.
/// </summary>
public class ActiveEffects
{
    public List<SpellEffect> Effects { get; set; } = new();

    public void AddEffect(SpellEffect effect)
    {
        Effects.Add(effect);
    }

    public void RemoveExpiredEffects()
    {
        Effects.RemoveAll(e => e.IsExpired());
    }

    public bool HasEffect(EffectType effectType)
    {
        return Effects.Any(e => e.EffectType == effectType && !e.IsExpired());
    }

    public SpellEffect? GetEffect(EffectType effectType)
    {
        return Effects.FirstOrDefault(e => e.EffectType == effectType && !e.IsExpired());
    }

    public void ReduceDurations(int amount = 1)
    {
        foreach (var effect in Effects)
        {
            effect.ReduceDuration(amount);
        }
    }
}
