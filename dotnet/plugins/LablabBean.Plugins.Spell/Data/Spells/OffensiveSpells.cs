namespace LablabBean.Plugins.Spell.Data.Spells;

/// <summary>
/// Offensive spell definitions.
/// </summary>
public static class OffensiveSpells
{
    public static Spell Fireball => new()
    {
        SpellId = "spell_fireball",
        Name = "Fireball",
        Description = "Hurl a ball of fire at your enemy, dealing fire damage.",
        ManaCost = 10,
        Cooldown = 3,
        BaseDamage = 15,
        SpellType = SpellType.Offensive,
        TargetType = TargetType.Single,
        RequiredLevel = 3,
        Effects = new List<SpellEffectData>
        {
            new() { EffectType = EffectType.Burn, Duration = 2, Intensity = 3, Description = "Burns target for 3 damage per turn" }
        }
    };

    public static Spell LightningBolt => new()
    {
        SpellId = "spell_lightning_bolt",
        Name = "Lightning Bolt",
        Description = "Strike your foe with a bolt of lightning, dealing high damage with a chance to stun.",
        ManaCost = 15,
        Cooldown = 4,
        BaseDamage = 20,
        SpellType = SpellType.Offensive,
        TargetType = TargetType.Single,
        RequiredLevel = 5,
        Effects = new List<SpellEffectData>
        {
            new() { EffectType = EffectType.Stun, Duration = 1, Intensity = 1, Description = "30% chance to stun target for 1 turn" }
        }
    };

    public static Spell IceShard => new()
    {
        SpellId = "spell_ice_shard",
        Name = "Ice Shard",
        Description = "Launch a shard of ice that damages and slows your enemy.",
        ManaCost = 8,
        Cooldown = 2,
        BaseDamage = 12,
        SpellType = SpellType.Offensive,
        TargetType = TargetType.Single,
        RequiredLevel = 2,
        Effects = new List<SpellEffectData>
        {
            new() { EffectType = EffectType.Slow, Duration = 3, Intensity = 20, Description = "Reduces target speed by 20 for 3 turns" }
        }
    };

    public static Spell MagicMissile => new()
    {
        SpellId = "spell_magic_missile",
        Name = "Magic Missile",
        Description = "A basic spell that always hits its target.",
        ManaCost = 5,
        Cooldown = 1,
        BaseDamage = 8,
        SpellType = SpellType.Offensive,
        TargetType = TargetType.Single,
        RequiredLevel = 1,
        Effects = new List<SpellEffectData>()
    };

    public static IEnumerable<Spell> GetAll()
    {
        yield return MagicMissile;
        yield return IceShard;
        yield return Fireball;
        yield return LightningBolt;
    }
}
