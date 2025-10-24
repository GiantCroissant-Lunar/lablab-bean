namespace LablabBean.Plugins.Spell.Data.Spells;

/// <summary>
/// Healing spell definitions.
/// </summary>
public static class HealingSpells
{
    public static Spell MinorHeal => new()
    {
        SpellId = "spell_minor_heal",
        Name = "Minor Heal",
        Description = "Restore a small amount of health.",
        ManaCost = 8,
        Cooldown = 3,
        BaseDamage = 15,
        SpellType = SpellType.Healing,
        TargetType = TargetType.Self,
        RequiredLevel = 1,
        Effects = new List<SpellEffectData>()
    };

    public static Spell Heal => new()
    {
        SpellId = "spell_heal",
        Name = "Heal",
        Description = "Restore a significant amount of health.",
        ManaCost = 12,
        Cooldown = 5,
        BaseDamage = 25,
        SpellType = SpellType.Healing,
        TargetType = TargetType.Self,
        RequiredLevel = 3,
        Effects = new List<SpellEffectData>()
    };

    public static Spell Regeneration => new()
    {
        SpellId = "spell_regeneration",
        Name = "Regeneration",
        Description = "Restore health over time.",
        ManaCost = 10,
        Cooldown = 6,
        BaseDamage = 0,
        SpellType = SpellType.Healing,
        TargetType = TargetType.Self,
        RequiredLevel = 4,
        Effects = new List<SpellEffectData>
        {
            new() { EffectType = EffectType.HealthRegen, Duration = 5, Intensity = 5, Description = "Restore 5 HP per turn for 5 turns" }
        }
    };

    public static IEnumerable<Spell> GetAll()
    {
        yield return MinorHeal;
        yield return Heal;
        yield return Regeneration;
    }
}
