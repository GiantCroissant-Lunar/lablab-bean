using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.Hazards.Components;
using LablabBean.Plugins.Hazards.Systems.Components;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Hazards.Systems;

/// <summary>
/// Handles hazard triggering and damage application
/// </summary>
public class HazardSystem
{
    private readonly World _world;
    private readonly ILogger<HazardSystem> _logger;
    private readonly Random _random = new();

    public HazardSystem(World world, ILogger<HazardSystem> logger)
    {
        _world = world;
        _logger = logger;
    }

    /// <summary>
    /// Process all active hazards (called each game turn)
    /// </summary>
    public void ProcessHazards()
    {
        ProcessPeriodicHazards();
        ProcessHazardEffects();
    }

    /// <summary>
    /// Check for hazard activation when entity enters a position
    /// </summary>
    public void CheckHazardActivation(int x, int y, Entity entity)
    {
        var query = new QueryDescription().WithAll<Hazard, Transform>();

        _world.Query(in query, (Entity hazardEntity, ref Hazard hazard, ref Transform hazardTransform) =>
        {
            if (hazard.State != HazardState.Active)
                return;

            // Check if hazard is at the same position
            if (hazardTransform.X != x || hazardTransform.Y != y)
                return;

            // Check trigger type
            var trigger = GetTrigger(hazardEntity);
            if (trigger.TriggerType != TriggerType.OnEnter)
                return;

            // Activation chance check
            if (_random.NextDouble() > hazard.ActivationChance)
                return;

            TriggerHazardInternal(hazardEntity, entity, hazard, trigger);
        });
    }

    /// <summary>
    /// Check proximity-based hazards
    /// </summary>
    public void CheckProximityHazards(int x, int y, Entity entity)
    {
        var query = new QueryDescription().WithAll<Hazard, HazardTrigger, Transform>();

        _world.Query(in query, (Entity hazardEntity, ref Hazard hazard, ref HazardTrigger trigger, ref Transform hazardTransform) =>
        {
            if (trigger.TriggerType != TriggerType.Proximity)
                return;

            if (hazard.State != HazardState.Active)
                return;

            var distance = Math.Abs(hazardTransform.X - x) + Math.Abs(hazardTransform.Y - y);

            if (distance <= trigger.ProximityRange)
            {
                TriggerHazardInternal(hazardEntity, entity, hazard, trigger);
            }
        });
    }

    private void ProcessPeriodicHazards()
    {
        var query = new QueryDescription().WithAll<Hazard, HazardTrigger, Transform>();
        var hazardsToTrigger = new List<(Entity hazardEntity, int x, int y)>();

        _world.Query(in query, (Entity hazardEntity, ref Hazard hazard, ref HazardTrigger trigger, ref Transform transform) =>
        {
            if (trigger.TriggerType != TriggerType.Periodic)
                return;

            if (hazard.State != HazardState.Active)
                return;

            trigger.TurnsSinceLastTrigger++;

            if (trigger.TurnsSinceLastTrigger >= trigger.Period)
            {
                hazardsToTrigger.Add((hazardEntity, transform.X, transform.Y));
                trigger.TurnsSinceLastTrigger = 0;
            }
        });

        // Find and damage entities at hazard positions
        foreach (var (hazardEntity, hazardX, hazardY) in hazardsToTrigger)
        {
            var entitiesQuery = new QueryDescription().WithAll<Transform>();

            _world.Query(in entitiesQuery, (Entity targetEntity, ref Transform targetTransform) =>
            {
                if (targetTransform.X == hazardX && targetTransform.Y == hazardY)
                {
                    var hazard = hazardEntity.Get<Hazard>();
                    var trigger = hazardEntity.Get<HazardTrigger>();
                    TriggerHazardInternal(hazardEntity, targetEntity, hazard, trigger);
                }
            });
        }
    }

    private void ProcessHazardEffects()
    {
        var query = new QueryDescription().WithAll<HazardEffect, Health>();
        var effectsToUpdate = new List<(Entity entity, HazardEffect effect, int damage)>();

        _world.Query(in query, (Entity entity, ref HazardEffect effect, ref Health health) =>
        {
            if (!entity.IsAlive())
                return;

            var resistance = GetResistance(entity, effect.SourceType);
            var actualDamage = (int)(effect.DamagePerTurn * (1 - resistance));

            effectsToUpdate.Add((entity, effect, actualDamage));
        });

        foreach (var (entity, effect, damage) in effectsToUpdate)
        {
            if (!entity.IsAlive())
                continue;

            // Apply damage
            var health = entity.Get<Health>();
            health.Current -= damage;
            entity.Set(in health);

            _logger.LogDebug($"Entity {entity.Id} takes {damage} damage from {effect.EffectName}");

            // Decrement duration
            var currentEffect = effect;
            currentEffect.RemainingTurns--;

            if (currentEffect.RemainingTurns <= 0)
            {
                entity.Remove<HazardEffect>();
                _logger.LogDebug($"{effect.EffectName} effect ended on entity {entity.Id}");
            }
            else
            {
                entity.Set(in currentEffect);
            }
        }
    }

    private void TriggerHazardInternal(Entity hazardEntity, Entity targetEntity, Hazard hazard, HazardTrigger trigger)
    {
        var newHazard = hazard;
        newHazard.State = HazardState.Triggered;

        var resistance = GetResistance(targetEntity, hazard.Type);
        var actualDamage = (int)(hazard.Damage * (1 - resistance));

        // Apply immediate damage
        if (targetEntity.Has<Health>())
        {
            var health = targetEntity.Get<Health>();
            health.Current -= actualDamage;
            targetEntity.Set(in health);

            _logger.LogInformation($"Hazard {hazard.Type} dealt {actualDamage} damage to entity {targetEntity.Id}");
        }

        // Apply ongoing effects for certain hazard types
        ApplyOngoingEffect(targetEntity, hazard.Type);

        // Check if hazard can retrigger
        if (trigger.CanRetrigger)
        {
            newHazard.State = HazardState.Active;
            var newTrigger = trigger;
            newTrigger.TurnsSinceLastTrigger = -trigger.RetriggerDelay;
            hazardEntity.Set(in newTrigger);
        }
        else
        {
            newHazard.State = HazardState.Active; // Default: can retrigger immediately
        }

        hazardEntity.Set(in newHazard);
    }

    private void ApplyOngoingEffect(Entity entity, HazardType hazardType)
    {
        var (duration, damagePerTurn, effectName) = hazardType switch
        {
            HazardType.Fire => (5, 2, "Burning"),
            HazardType.Lava => (3, 5, "Burning"),
            HazardType.PoisonGas => (10, 1, "Poisoned"),
            HazardType.AcidPool => (5, 3, "Corroding"),
            _ => (0, 0, "None")
        };

        if (duration > 0)
        {
            var effect = new HazardEffect(hazardType, damagePerTurn, duration, effectName);
            entity.Set(in effect);
            _logger.LogDebug($"Applied {effectName} effect to entity {entity.Id}");
        }
    }

    private float GetResistance(Entity entity, HazardType hazardType)
    {
        if (!entity.Has<HazardResistance>())
            return 0f;

        var resistance = entity.Get<HazardResistance>();
        return resistance.GetResistance(hazardType);
    }

    private HazardTrigger GetTrigger(Entity hazardEntity)
    {
        if (!hazardEntity.Has<HazardTrigger>())
            return new HazardTrigger(TriggerType.OnEnter);

        return hazardEntity.Get<HazardTrigger>();
    }
}
