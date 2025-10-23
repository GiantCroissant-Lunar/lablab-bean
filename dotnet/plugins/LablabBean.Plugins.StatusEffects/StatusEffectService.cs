using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Game.Core.Components;
using Microsoft.Extensions.Logging;
using StatusEffectsComponent = LablabBean.Game.Core.Components.StatusEffects;

namespace LablabBean.Plugins.StatusEffects.Services;

/// <summary>
/// Implementation of status effect service - encapsulates all status effect logic
/// </summary>
public class StatusEffectService : IStatusEffectService
{
    private readonly ILogger _logger;

    public StatusEffectService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public EffectResult ApplyEffect(World world, Entity entity, EffectDefinition definition, EffectSource source)
    {
        return ApplyEffect(world, entity, definition.Type, definition.DefaultMagnitude, definition.DefaultDuration, source);
    }

    public EffectResult ApplyEffect(World world, Entity entity, EffectType effectType, int magnitude, int duration, EffectSource source)
    {
        if (!world.IsAlive(entity))
            return EffectResult.Failed("Entity no longer exists");

        if (duration <= 0)
            return EffectResult.Failed("Effect duration must be at least 1 turn");

        // Ensure entity has StatusEffects component
        if (!world.Has<StatusEffectsComponent>(entity))
        {
            world.Add(entity, StatusEffectsComponent.CreateEmpty());
        }

        ref var statusEffects = ref world.Get<StatusEffectsComponent>(entity);

        // Check if max effects reached
        if (statusEffects.IsFull)
        {
            // Check if we're refreshing an existing effect
            var existingEffect = statusEffects.ActiveEffects.FirstOrDefault(e => e.Type == effectType);
            if (existingEffect.Type == EffectType.Poison && existingEffect.Type == default)
            {
                return EffectResult.Failed("Cannot apply effect: maximum effects reached");
            }
        }

        var definition = EffectDefinitions.GetDefinition(effectType);

        // Check if effect already exists (refresh duration instead of stacking)
        var existingIndex = statusEffects.ActiveEffects.FindIndex(e => e.Type == effectType);
        if (existingIndex >= 0)
        {
            var existing = statusEffects.ActiveEffects[existingIndex];
            existing.Duration = duration;
            statusEffects.ActiveEffects[existingIndex] = existing;

            _logger.LogDebug($"Refreshed {effectType} on entity {entity.Id} (duration reset to {duration})");
            return EffectResult.Succeeded(GetEffectAppliedMessage(effectType, definition.Category, isRefresh: true));
        }

        // Add new effect
        var newEffect = new StatusEffect
        {
            Type = effectType,
            Magnitude = magnitude,
            Duration = duration,
            Category = definition.Category,
            Source = source,
            Color = definition.Color
        };

        statusEffects.ActiveEffects.Add(newEffect);
        _logger.LogDebug($"Applied {effectType} to entity {entity.Id} (mag:{magnitude}, dur:{duration})");

        return EffectResult.Succeeded(GetEffectAppliedMessage(effectType, definition.Category, isRefresh: false));
    }

    public EffectResult RemoveEffect(World world, Entity entity, EffectType effectType)
    {
        if (!world.IsAlive(entity) || !world.Has<StatusEffectsComponent>(entity))
            return EffectResult.Failed($"Not affected by {effectType}");

        ref var statusEffects = ref world.Get<StatusEffectsComponent>(entity);
        var removed = statusEffects.ActiveEffects.RemoveAll(e => e.Type == effectType);

        if (removed > 0)
        {
            _logger.LogDebug($"Removed {effectType} from entity {entity.Id}");
            return EffectResult.Succeeded($"{effectType} removed!");
        }

        return EffectResult.Failed($"Not affected by {effectType}");
    }

    public EffectResult RemoveAllNegativeEffects(World world, Entity entity)
    {
        if (!world.IsAlive(entity) || !world.Has<StatusEffectsComponent>(entity))
            return EffectResult.Failed("No negative effects active");

        ref var statusEffects = ref world.Get<StatusEffectsComponent>(entity);
        var removed = statusEffects.ActiveEffects.RemoveAll(e =>
            e.Category == EffectCategory.DamageOverTime ||
            e.Category == EffectCategory.StatDebuff);

        if (removed > 0)
        {
            _logger.LogDebug($"Removed {removed} negative effects from entity {entity.Id}");
            return EffectResult.Succeeded("All negative effects removed!");
        }

        return EffectResult.Failed("No negative effects active");
    }

    public void ClearAllEffects(World world, Entity entity)
    {
        if (!world.IsAlive(entity) || !world.Has<StatusEffectsComponent>(entity))
            return;

        ref var statusEffects = ref world.Get<StatusEffectsComponent>(entity);
        statusEffects.ActiveEffects.Clear();
        _logger.LogDebug($"Cleared all effects from entity {entity.Id}");
    }

    public List<string> ProcessEffects(World world, Entity entity)
    {
        var messages = new List<string>();

        if (!world.IsAlive(entity) || !world.Has<StatusEffectsComponent>(entity))
            return messages;

        ref var statusEffects = ref world.Get<StatusEffectsComponent>(entity);
        var effectsToRemove = new List<int>();

        // Process each effect
        for (int i = 0; i < statusEffects.ActiveEffects.Count; i++)
        {
            var effect = statusEffects.ActiveEffects[i];

            // Apply effect based on category
            switch (effect.Category)
            {
                case EffectCategory.DamageOverTime:
                    messages.Add(ApplyDamageOverTime(world, entity, effect));
                    break;

                case EffectCategory.HealingOverTime:
                    messages.Add(ApplyHealingOverTime(world, entity, effect));
                    break;
            }

            // Decrement duration
            effect.Duration--;
            statusEffects.ActiveEffects[i] = effect;

            // Mark for removal if expired
            if (effect.IsExpired)
            {
                effectsToRemove.Add(i);
                messages.Add(GetEffectExpiredMessage(effect.Type));
            }
        }

        // Remove expired effects (reverse order to maintain indices)
        for (int i = effectsToRemove.Count - 1; i >= 0; i--)
        {
            statusEffects.ActiveEffects.RemoveAt(effectsToRemove[i]);
        }

        return messages;
    }

    public (int attackMod, int defenseMod, int speedMod) CalculateStatModifiers(World world, Entity entity)
    {
        if (!world.IsAlive(entity) || !world.Has<StatusEffectsComponent>(entity))
            return (0, 0, 0);

        var statusEffects = world.Get<StatusEffectsComponent>(entity);
        int attackMod = 0, defenseMod = 0, speedMod = 0;

        foreach (var effect in statusEffects.ActiveEffects)
        {
            if (effect.Category != EffectCategory.StatBuff && effect.Category != EffectCategory.StatDebuff)
                continue;

            int magnitude = effect.Magnitude;
            if (effect.Category == EffectCategory.StatDebuff)
                magnitude = -magnitude;

            switch (effect.Type)
            {
                case EffectType.Strength:
                    attackMod += magnitude;
                    break;
                case EffectType.Weakness:
                    attackMod -= effect.Magnitude;
                    break;
                case EffectType.IronSkin:
                    defenseMod += magnitude;
                    break;
                case EffectType.Fragile:
                    defenseMod -= effect.Magnitude;
                    break;
                case EffectType.Haste:
                    speedMod += magnitude;
                    break;
                case EffectType.Slow:
                    speedMod -= effect.Magnitude;
                    break;
            }
        }

        return (attackMod, defenseMod, speedMod);
    }

    public List<ActiveEffectInfo> GetActiveEffects(World world, Entity entity)
    {
        var activeEffects = new List<ActiveEffectInfo>();

        if (!world.IsAlive(entity) || !world.Has<StatusEffectsComponent>(entity))
            return activeEffects;

        var statusEffects = world.Get<StatusEffectsComponent>(entity);

        foreach (var effect in statusEffects.ActiveEffects)
        {
            var definition = EffectDefinitions.GetDefinition(effect.Type);
            activeEffects.Add(new ActiveEffectInfo(
                effect.Type,
                effect.DisplayName,
                effect.Magnitude,
                effect.Duration,
                effect.Category,
                effect.Source,
                effect.Color.ToString(),
                definition.Description
            ));
        }

        return activeEffects;
    }

    public bool HasEffect(World world, Entity entity, EffectType effectType)
    {
        if (!world.IsAlive(entity) || !world.Has<StatusEffectsComponent>(entity))
            return false;

        var statusEffects = world.Get<StatusEffectsComponent>(entity);
        return statusEffects.ActiveEffects.Any(e => e.Type == effectType);
    }

    public int GetEffectDuration(World world, Entity entity, EffectType effectType)
    {
        if (!world.IsAlive(entity) || !world.Has<StatusEffectsComponent>(entity))
            return 0;

        var statusEffects = world.Get<StatusEffectsComponent>(entity);
        var effect = statusEffects.ActiveEffects.FirstOrDefault(e => e.Type == effectType);
        return effect.Type == effectType ? effect.Duration : 0;
    }

    // Private helper methods

    private string ApplyDamageOverTime(World world, Entity entity, StatusEffect effect)
    {
        if (!world.Has<Health>(entity))
            return string.Empty;

        ref var health = ref world.Get<Health>(entity);
        int oldHealth = health.Current;
        health.Current = Math.Max(0, health.Current - effect.Magnitude);
        int actualDamage = oldHealth - health.Current;

        bool isPlayer = world.Has<Player>(entity);
        string subject = isPlayer ? "You take" : "Enemy takes";

        return $"{subject} {actualDamage} damage from {effect.Type.ToString().ToLower()}.";
    }

    private string ApplyHealingOverTime(World world, Entity entity, StatusEffect effect)
    {
        if (!world.Has<Health>(entity))
            return string.Empty;

        ref var health = ref world.Get<Health>(entity);
        int oldHealth = health.Current;
        health.Current = Math.Min(health.Maximum, health.Current + effect.Magnitude);
        int actualHealing = health.Current - oldHealth;

        if (actualHealing == 0)
            return string.Empty;

        bool isPlayer = world.Has<Player>(entity);
        string subject = isPlayer ? "You heal" : "Enemy heals";

        return $"{subject} {actualHealing} HP from {effect.Type.ToString().ToLower()}.";
    }

    private string GetEffectAppliedMessage(EffectType type, EffectCategory category, bool isRefresh)
    {
        if (isRefresh)
            return $"{type} duration refreshed!";

        return type switch
        {
            EffectType.Poison => "You are poisoned!",
            EffectType.Bleed => "You are bleeding!",
            EffectType.Burning => "You are burning!",
            EffectType.Regeneration => "Regeneration active!",
            EffectType.Blessed => "You feel blessed!",
            EffectType.Strength => "Strength increased!",
            EffectType.Haste => "Movement accelerated!",
            EffectType.IronSkin => "Defense increased!",
            EffectType.Weakness => "You feel weakened!",
            EffectType.Slow => "You feel sluggish!",
            EffectType.Fragile => "Defense decreased!",
            _ => $"{type} applied!"
        };
    }

    private string GetEffectExpiredMessage(EffectType type)
    {
        return $"{type} has worn off.";
    }
}
