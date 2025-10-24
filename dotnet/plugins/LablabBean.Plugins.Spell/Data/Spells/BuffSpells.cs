namespace LablabBean.Plugins.Spell.Data.Spells;

/// <summary>
/// Buff spell definitions.
/// </summary>
public static class BuffSpells
{
    public static Spell Shield => new()
    {
        SpellId = "spell_shield",
        Name = "Shield",
        Description = "Increase your defense for a short time.",
        ManaCost = 10,
        Cooldown = 6,
        BaseDamage = 0,
        SpellType = SpellType.Buff,
        TargetType = TargetType.Self,
        RequiredLevel = 2,
        Effects = new List<SpellEffectData>
        {
            new() { EffectType = EffectType.DefenseBoost, Duration = 3, Intensity = 5, Description = "+5 Defense for 3 turns" }
        }
    };

    public static Spell Haste => new()
    {
        SpellId = "spell_haste",
        Name = "Haste",
        Description = "Increase your speed for a short time.",
        ManaCost = 8,
        Cooldown = 5,
        BaseDamage = 0,
        SpellType = SpellType.Buff,
        TargetType = TargetType.Self,
        RequiredLevel = 3,
        Effects = new List<SpellEffectData>
        {
            new() { EffectType = EffectType.SpeedBoost, Duration = 3, Intensity = 20, Description = "+20 Speed for 3 turns" }
        }
    };

    public static Spell PowerUp => new()
    {
        SpellId = "spell_power_up",
        Name = "Power Up",
        Description = "Increase your attack power for a short time.",
        ManaCost = 12,
        Cooldown = 6,
        BaseDamage = 0,
        SpellType = SpellType.Buff,
        TargetType = TargetType.Self,
        RequiredLevel = 4,
        Effects = new List<SpellEffectData>
        {
            new() { EffectType = EffectType.AttackBoost, Duration = 3, Intensity = 8, Description = "+8 Attack for 3 turns" }
        }
    };

    public static IEnumerable<Spell> GetAll()
    {
        yield return Shield;
        yield return Haste;
        yield return PowerUp;
    }
}
