using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Game.Core.Components;
using LablabBean.Plugins.Spells.Data;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Spells.Systems;

/// <summary>
/// System for applying spell effects to targets.
/// </summary>
public class SpellEffectSystem
{
    private readonly World _world;
    private readonly ILogger _logger;

    public SpellEffectSystem(World world, ILogger logger)
    {
        _world = world;
        _logger = logger;
    }

    public (int damage, int healing) ApplyEffects(
        Entity caster,
        Entity target,
        List<SpellEffect> effects)
    {
        int totalDamage = 0;
        int totalHealing = 0;

        foreach (var effect in effects)
        {
            switch (effect.Type)
            {
                case SpellEffectType.Damage:
                    totalDamage += ApplyDamage(target, effect.Value);
                    break;

                case SpellEffectType.Heal:
                    totalHealing += ApplyHealing(target, effect.Value);
                    break;

                case SpellEffectType.Shield:
                    ApplyShield(target, effect.Value, effect.Duration);
                    break;

                case SpellEffectType.Buff:
                    ApplyBuff(target, effect.Value, effect.Duration);
                    break;

                case SpellEffectType.Debuff:
                    ApplyDebuff(target, effect.Value, effect.Duration);
                    break;

                case SpellEffectType.StatusEffect:
                    ApplyStatusEffect(target, effect.StatusEffectId!, effect.Duration);
                    break;

                case SpellEffectType.Teleport:
                    // TODO: Implement teleport effect
                    _logger.LogWarning("Teleport effect not yet implemented");
                    break;

                case SpellEffectType.Reveal:
                    // TODO: Implement reveal effect (fog of war)
                    _logger.LogWarning("Reveal effect not yet implemented");
                    break;
            }
        }

        return (totalDamage, totalHealing);
    }

    private int ApplyDamage(Entity target, int amount)
    {
        if (!target.Has<Health>()) return 0;

        ref var health = ref target.Get<Health>();
        var actualDamage = Math.Min(amount, health.Current);
        health.Current -= actualDamage;

        _logger.LogDebug("Applied {Damage} damage to entity {Target}", actualDamage, target);

        return actualDamage;
    }

    private int ApplyHealing(Entity target, int amount)
    {
        if (!target.Has<Health>()) return 0;

        ref var health = ref target.Get<Health>();
        var beforeHealing = health.Current;
        health.Current = Math.Min(health.Current + amount, health.Maximum);
        var actualHealing = health.Current - beforeHealing;

        _logger.LogDebug("Applied {Healing} healing to entity {Target}", actualHealing, target);

        return actualHealing;
    }

    private void ApplyShield(Entity target, int amount, int duration)
    {
        // Shield implementation would integrate with StatusEffects system
        // For now, add temporary defense boost
        if (!target.Has<Combat>()) return;

        ref var combat = ref target.Get<Combat>();
        combat.Defense += amount;

        _logger.LogDebug("Applied {Shield} shield to entity {Target} for {Duration} turns",
            amount, target, duration);

        // TODO: Add timed removal of shield
    }

    private void ApplyBuff(Entity target, int amount, int duration)
    {
        if (!target.Has<Combat>()) return;

        ref var combat = ref target.Get<Combat>();
        combat.Attack += amount;

        _logger.LogDebug("Applied {Buff} attack buff to entity {Target} for {Duration} turns",
            amount, target, duration);

        // TODO: Add timed removal of buff
    }

    private void ApplyDebuff(Entity target, int amount, int duration)
    {
        if (!target.Has<Combat>()) return;

        ref var combat = ref target.Get<Combat>();
        combat.Attack = Math.Max(0, combat.Attack - amount);

        _logger.LogDebug("Applied {Debuff} attack debuff to entity {Target} for {Duration} turns",
            amount, target, duration);

        // TODO: Add timed removal of debuff
    }

    private void ApplyStatusEffect(Entity target, string effectId, int duration)
    {
        // This would integrate with the StatusEffects plugin
        _logger.LogDebug("Applied status effect {EffectId} to entity {Target} for {Duration} turns",
            effectId, target, duration);

        // TODO: Integrate with StatusEffects plugin
    }
}
