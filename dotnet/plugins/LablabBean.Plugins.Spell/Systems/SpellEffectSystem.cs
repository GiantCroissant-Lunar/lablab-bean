using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.Spell.Components;
using LablabBean.Plugins.Spell.Data;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Spell.Systems;

/// <summary>
/// System for processing active spell effects (DoT, buffs, debuffs).
/// </summary>
public class SpellEffectSystem
{
    private readonly World _world;
    private readonly ILogger _logger;
    private readonly QueryDescription _effectQuery;

    public SpellEffectSystem(World world, ILogger logger)
    {
        _world = world;
        _logger = logger;
        _effectQuery = new QueryDescription().WithAll<ActiveEffects>();
    }

    public void Update(float deltaTime)
    {
        _world.Query(in _effectQuery, (Entity entity, ref ActiveEffects effects) =>
        {
            ProcessEffects(entity, effects);

            effects.ReduceDurations();
            effects.RemoveExpiredEffects();
        });
    }

    private void ProcessEffects(Entity entity, ActiveEffects effects)
    {
        foreach (var effect in effects.Effects)
        {
            if (effect.IsExpired()) continue;

            switch (effect.EffectType)
            {
                case EffectType.Burn:
                    ApplyBurnDamage(entity, effect.Intensity);
                    break;
                case EffectType.HealthRegen:
                    ApplyHealing(entity, effect.Intensity);
                    break;
                case EffectType.Shield:
                    break;
                case EffectType.AttackBoost:
                case EffectType.DefenseBoost:
                case EffectType.SpeedBoost:
                    break;
            }
        }
    }

    private void ApplyBurnDamage(Entity entity, int damage)
    {
        _logger.LogDebug("Entity {Entity} takes {Damage} burn damage", entity, damage);
    }

    private void ApplyHealing(Entity entity, int healing)
    {
        _logger.LogDebug("Entity {Entity} regenerates {Healing} HP", entity, healing);
    }
}
