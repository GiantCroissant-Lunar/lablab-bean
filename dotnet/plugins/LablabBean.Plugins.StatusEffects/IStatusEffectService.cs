using Arch.Core;
using LablabBean.Game.Core.Components;

namespace LablabBean.Plugins.StatusEffects.Services;

/// <summary>
/// Public service interface for status effect operations.
/// Exposed to host and other plugins via DI registry.
/// </summary>
public interface IStatusEffectService
{
    /// <summary>
    /// Apply a status effect to an entity using predefined definition
    /// </summary>
    EffectResult ApplyEffect(World world, Entity entity, EffectDefinition definition, EffectSource source);

    /// <summary>
    /// Apply a status effect to an entity with custom magnitude and duration
    /// </summary>
    EffectResult ApplyEffect(World world, Entity entity, EffectType effectType, int magnitude, int duration, EffectSource source);

    /// <summary>
    /// Remove a specific effect type from an entity
    /// </summary>
    EffectResult RemoveEffect(World world, Entity entity, EffectType effectType);

    /// <summary>
    /// Remove all negative effects (debuffs and DoT) from an entity
    /// </summary>
    EffectResult RemoveAllNegativeEffects(World world, Entity entity);

    /// <summary>
    /// Clear all effects from an entity (used on death)
    /// </summary>
    void ClearAllEffects(World world, Entity entity);

    /// <summary>
    /// Process all effects for an entity at turn start
    /// Returns feedback messages for display
    /// </summary>
    List<string> ProcessEffects(World world, Entity entity);

    /// <summary>
    /// Calculate total stat modifiers from all active effects
    /// Returns (attackMod, defenseMod, speedMod)
    /// </summary>
    (int attackMod, int defenseMod, int speedMod) CalculateStatModifiers(World world, Entity entity);

    /// <summary>
    /// Get all active effects on an entity (read model for UI)
    /// </summary>
    List<ActiveEffectInfo> GetActiveEffects(World world, Entity entity);

    /// <summary>
    /// Check if an entity has a specific effect active
    /// </summary>
    bool HasEffect(World world, Entity entity, EffectType effectType);

    /// <summary>
    /// Get the remaining duration of a specific effect
    /// Returns 0 if effect is not active
    /// </summary>
    int GetEffectDuration(World world, Entity entity, EffectType effectType);
}

/// <summary>
/// Read model for active effect information
/// </summary>
public record ActiveEffectInfo(
    EffectType Type,
    string Name,
    int Magnitude,
    int Duration,
    EffectCategory Category,
    EffectSource Source,
    string Color,
    string Description);

/// <summary>
/// Read model for stat modifiers
/// </summary>
public record StatModifiers(int AttackModifier, int DefenseModifier, int SpeedModifier)
{
    public bool HasModifiers => AttackModifier != 0 || DefenseModifier != 0 || SpeedModifier != 0;
}

/// <summary>
/// Events published by status effects plugin to host
/// </summary>
public static class StatusEffectEvents
{
    public const string EffectApplied = "StatusEffects.EffectApplied";
    public const string EffectRemoved = "StatusEffects.EffectRemoved";
    public const string EffectExpired = "StatusEffects.EffectExpired";
    public const string EffectProcessed = "StatusEffects.EffectProcessed";
    public const string EffectsChanged = "StatusEffects.Changed";
}

/// <summary>
/// Event data for effect applied
/// </summary>
public record EffectAppliedEvent(int EntityId, EffectType Type, int Magnitude, int Duration, bool IsRefresh);

/// <summary>
/// Event data for effect removed
/// </summary>
public record EffectRemovedEvent(int EntityId, EffectType Type, bool WasExpired);

/// <summary>
/// Event data for effect processed (DoT/HoT tick)
/// </summary>
public record EffectProcessedEvent(int EntityId, EffectType Type, int Value, string Message);

/// <summary>
/// Event data for effects changed
/// </summary>
public record EffectsChangedEvent(int EntityId, int ActiveEffectCount, bool HasNegativeEffects);

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
