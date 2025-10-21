using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Game.Core.Components;
using Microsoft.Extensions.Logging;

namespace LablabBean.Game.Core.Systems;

/// <summary>
/// System that manages status effects (buffs, debuffs, DoT, HoT)
/// </summary>
public class StatusEffectSystem
{
    private readonly ILogger<StatusEffectSystem> _logger;

    public StatusEffectSystem(ILogger<StatusEffectSystem> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Apply a status effect to an entity using predefined definition
    /// </summary>
    public EffectResult ApplyEffect(World world, Entity entity, EffectDefinition definition, EffectSource source)
    {
        return ApplyEffect(world, entity, definition.Type, definition.DefaultMagnitude, definition.DefaultDuration, source);
    }

    /// <summary>
    /// Apply a status effect to an entity with custom magnitude and duration
    /// </summary>
    public EffectResult ApplyEffect(World world, Entity entity, EffectType effectType, int magnitude, int duration, EffectSource source)
    {
        if (!world.IsAlive(entity))
            return EffectResult.Failed("Entity no longer exists");

        if (duration <= 0)
            return EffectResult.Failed("Effect duration must be at least 1 turn");

        // Ensure entity has StatusEffects component
        if (!world.Has<StatusEffects>(entity))
        {
            world.Add(entity, StatusEffects.CreateEmpty());
        }

        ref var statusEffects = ref world.Get<StatusEffects>(entity);

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

    /// <summary>
    /// Remove a specific effect type from an entity
    /// </summary>
    public EffectResult RemoveEffect(World world, Entity entity, EffectType effectType)
    {
        if (!world.IsAlive(entity) || !world.Has<StatusEffects>(entity))
            return EffectResult.Failed($"Not affected by {effectType}");

        ref var statusEffects = ref world.Get<StatusEffects>(entity);
        var removed = statusEffects.ActiveEffects.RemoveAll(e => e.Type == effectType);

        if (removed > 0)
        {
            _logger.LogDebug($"Removed {effectType} from entity {entity.Id}");
            return EffectResult.Succeeded($"{effectType} removed!");
        }

        return EffectResult.Failed($"Not affected by {effectType}");
    }

    /// <summary>
    /// Remove all negative effects (debuffs and DoT) from an entity
    /// </summary>
    public EffectResult RemoveAllNegativeEffects(World world, Entity entity)
    {
        if (!world.IsAlive(entity) || !world.Has<StatusEffects>(entity))
            return EffectResult.Failed("No negative effects active");

        ref var statusEffects = ref world.Get<StatusEffects>(entity);
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

    /// <summary>
    /// Clear all effects from an entity (used on death)
    /// </summary>
    public void ClearAllEffects(World world, Entity entity)
    {
        if (!world.IsAlive(entity) || !world.Has<StatusEffects>(entity))
            return;

        ref var statusEffects = ref world.Get<StatusEffects>(entity);
        statusEffects.ActiveEffects.Clear();
        _logger.LogDebug($"Cleared all effects from entity {entity.Id}");
    }

    /// <summary>
    /// Process all effects for an entity at turn start
    /// Returns feedback messages for display
    /// </summary>
    public List<string> ProcessEffects(World world, Entity entity)
    {
        var messages = new List<string>();

        if (!world.IsAlive(entity) || !world.Has<StatusEffects>(entity))
            return messages;

        ref var statusEffects = ref world.Get<StatusEffects>(entity);
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

    /// <summary>
    /// Calculate total stat modifiers from all active effects
    /// Returns (attackMod, defenseMod, speedMod)
    /// </summary>
    public (int attackMod, int defenseMod, int speedMod) CalculateStatModifiers(World world, Entity entity)
    {
        if (!world.IsAlive(entity) || !world.Has<StatusEffects>(entity))
            return (0, 0, 0);

        var statusEffects = world.Get<StatusEffects>(entity);
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

    /// <summary>
    /// Calculate total stats including base and status effects
    /// Equipment bonuses should be handled by InventorySystem
    /// </summary>
    public (int attack, int defense, int speed) CalculateTotalStats(World world, Entity entity)
    {
        // Get base stats
        int baseAttack = 0, baseDefense = 0, baseSpeed = 100;

        if (world.Has<Actor>(entity))
        {
            var actor = world.Get<Actor>(entity);
            baseSpeed = actor.Speed;
        }

        if (world.Has<Combat>(entity))
        {
            var combat = world.Get<Combat>(entity);
            baseAttack = combat.Attack;
            baseDefense = combat.Defense;
        }

        // Get status effect modifiers
        var (effectAttack, effectDefense, effectSpeed) = CalculateStatModifiers(world, entity);

        // Calculate totals with minimums
        int totalAttack = Math.Max(1, baseAttack + effectAttack);
        int totalDefense = Math.Max(1, baseDefense + effectDefense);
        int totalSpeed = Math.Max(1, baseSpeed + effectSpeed);

        return (totalAttack, totalDefense, totalSpeed);
    }

    /// <summary>
    /// Check if entity has a specific effect active
    /// </summary>
    public bool HasEffect(World world, Entity entity, EffectType effectType)
    {
        if (!world.IsAlive(entity) || !world.Has<StatusEffects>(entity))
            return false;

        var statusEffects = world.Get<StatusEffects>(entity);
        return statusEffects.ActiveEffects.Any(e => e.Type == effectType);
    }

    /// <summary>
    /// Get all active effects for an entity
    /// </summary>
    public List<StatusEffect> GetActiveEffects(World world, Entity entity)
    {
        if (!world.IsAlive(entity) || !world.Has<StatusEffects>(entity))
            return new List<StatusEffect>();

        var statusEffects = world.Get<StatusEffects>(entity);
        return new List<StatusEffect>(statusEffects.ActiveEffects);
    }

    /// <summary>
    /// Check if effect can be applied (not at max effects, unless refreshing existing)
    /// </summary>
    public bool CanApplyEffect(World world, Entity entity, EffectType effectType)
    {
        if (!world.IsAlive(entity))
            return false;

        if (!world.Has<StatusEffects>(entity))
            return true;

        var statusEffects = world.Get<StatusEffects>(entity);
        
        // Can always refresh existing effect
        if (statusEffects.ActiveEffects.Any(e => e.Type == effectType))
            return true;

        // Check if room for new effect
        return !statusEffects.IsFull;
    }

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

    public string FormatEffectForDisplay(StatusEffect effect)
    {
        string colorCode = effect.Color switch
        {
            EffectColor.Red => "\x1b[31m",
            EffectColor.Green => "\x1b[32m",
            EffectColor.Blue => "\x1b[34m",
            EffectColor.Cyan => "\x1b[36m",
            EffectColor.Yellow => "\x1b[33m",
            EffectColor.Orange => "\x1b[38;5;208m",
            EffectColor.DarkRed => "\x1b[38;5;88m",
            EffectColor.Purple => "\x1b[35m",
            _ => "\x1b[0m"
        };
        
        string reset = "\x1b[0m";
        return $"{colorCode}{effect.DisplayName} ({effect.Duration}){reset}";
    }
}

/// <summary>
/// Result of a status effect operation
/// </summary>
public struct EffectResult
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public static EffectResult Succeeded(string message) => new() { Success = true, Message = message };
    public static EffectResult Failed(string message) => new() { Success = false, Message = message };
}
