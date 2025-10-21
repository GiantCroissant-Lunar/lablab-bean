// Contract: StatusEffectSystem
// Purpose: Defines the interface for status effect management
// Location: LablabBean.Game.Core/Systems/StatusEffectSystem.cs

namespace LablabBean.Game.Core.Systems;

using Arch.Core;
using LablabBean.Game.Core.Components;

/// <summary>
/// System responsible for managing status effects on entities.
/// Handles application, removal, duration tracking, and stat modification.
/// </summary>
public class StatusEffectSystem
{
    private readonly World _world;

    public StatusEffectSystem(World world)
    {
        _world = world;
    }

    // ═══════════════════════════════════════════════════════════════
    // EFFECT APPLICATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Applies a status effect to an entity.
    /// </summary>
    /// <param name="targetEntity">The entity to affect</param>
    /// <param name="effectType">Type of effect to apply</param>
    /// <param name="magnitude">Effect magnitude (damage, stat bonus, etc.)</param>
    /// <param name="duration">Duration in turns (1-99)</param>
    /// <param name="source">Source of the effect</param>
    /// <returns>Result indicating success or failure reason</returns>
    /// <remarks>
    /// Preconditions:
    /// - Target must have StatusEffects component
    /// - Duration must be 1-99 turns
    /// - Magnitude must be non-zero
    /// - StatusEffects.Count must be < MaxEffects
    /// 
    /// Stacking Rules:
    /// - If same effect type already active: refresh duration to new value
    /// - If different effect type: add as new effect
    /// - Magnitude does NOT stack (prevents abuse)
    /// 
    /// Effects:
    /// - Adds StatusEffect to StatusEffects.ActiveEffects list
    /// - Returns feedback message ("You are poisoned!", "Strength increased!")
    /// </remarks>
    public StatusEffectResult ApplyEffect(
        Entity targetEntity,
        EffectType effectType,
        int magnitude,
        int duration,
        EffectSource source);

    /// <summary>
    /// Applies a status effect using a predefined effect definition.
    /// </summary>
    /// <param name="targetEntity">The entity to affect</param>
    /// <param name="definition">Effect definition with default values</param>
    /// <param name="source">Source of the effect</param>
    /// <returns>Result indicating success or failure reason</returns>
    public StatusEffectResult ApplyEffect(
        Entity targetEntity,
        EffectDefinition definition,
        EffectSource source);

    /// <summary>
    /// Checks if an effect can be applied to an entity.
    /// </summary>
    /// <param name="targetEntity">The entity to check</param>
    /// <param name="effectType">Type of effect</param>
    /// <returns>True if effect can be applied, false otherwise</returns>
    public bool CanApplyEffect(Entity targetEntity, EffectType effectType);

    // ═══════════════════════════════════════════════════════════════
    // EFFECT REMOVAL
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Removes a specific status effect from an entity.
    /// </summary>
    /// <param name="targetEntity">The entity to affect</param>
    /// <param name="effectType">Type of effect to remove</param>
    /// <returns>Result indicating success or failure reason</returns>
    /// <remarks>
    /// Preconditions:
    /// - Target must have StatusEffects component
    /// - Effect of specified type must be active
    /// 
    /// Effects:
    /// - Removes first matching effect from ActiveEffects list
    /// - Returns feedback message ("Poison cured!")
    /// </remarks>
    public StatusEffectResult RemoveEffect(Entity targetEntity, EffectType effectType);

    /// <summary>
    /// Removes all negative effects (debuffs and DoT) from an entity.
    /// </summary>
    /// <param name="targetEntity">The entity to affect</param>
    /// <returns>Result indicating success or failure reason</returns>
    /// <remarks>
    /// Used by "Universal Cure" items.
    /// Removes all effects with category DamageOverTime or StatDebuff.
    /// </remarks>
    public StatusEffectResult RemoveAllNegativeEffects(Entity targetEntity);

    /// <summary>
    /// Clears all status effects from an entity.
    /// </summary>
    /// <param name="targetEntity">The entity to affect</param>
    /// <remarks>
    /// Called when entity dies.
    /// Immediately removes all effects without feedback messages.
    /// </remarks>
    public void ClearAllEffects(Entity targetEntity);

    // ═══════════════════════════════════════════════════════════════
    // EFFECT PROCESSING (TURN-BASED)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Processes all status effects for an entity at the start of their turn.
    /// </summary>
    /// <param name="entity">The entity whose turn is starting</param>
    /// <returns>List of feedback messages for effects that ticked</returns>
    /// <remarks>
    /// Processing Order:
    /// 1. Apply DoT effects (Poison, Bleed, Burning) - damage Health
    /// 2. Apply HoT effects (Regeneration, Blessed) - heal Health
    /// 3. Decrement all effect durations by 1
    /// 4. Remove expired effects (duration <= 0)
    /// 5. Return feedback messages for all processed effects
    /// 
    /// Called by ActorSystem at the start of each entity's turn.
    /// </remarks>
    public List<string> ProcessEffects(Entity entity);

    /// <summary>
    /// Processes status effects for all entities with active effects.
    /// </summary>
    /// <remarks>
    /// Batch processing version for efficiency.
    /// Queries all entities with StatusEffects component.
    /// </remarks>
    public void ProcessAllEffects();

    // ═══════════════════════════════════════════════════════════════
    // STAT CALCULATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates total stat modifiers from all active status effects.
    /// </summary>
    /// <param name="entity">The entity to calculate for</param>
    /// <returns>Tuple of (attackMod, defenseMod, speedMod)</returns>
    /// <remarks>
    /// Sums all stat-modifying effects:
    /// - Strength/Weakness → attack modifier
    /// - IronSkin/Fragile → defense modifier
    /// - Haste/Slow → speed modifier
    /// 
    /// Multiple effects stack additively:
    /// - Strength (+5) + Strength (+5) = +5 (same type refreshes, doesn't stack)
    /// - Strength (+5) + Weakness (-3) = +2 (different types stack)
    /// 
    /// Used by CombatSystem and ActorSystem.
    /// </remarks>
    public (int attackMod, int defenseMod, int speedMod) CalculateStatModifiers(Entity entity);

    /// <summary>
    /// Calculates total combat stats including base, equipment, and status effects.
    /// </summary>
    /// <param name="entity">The entity to calculate for</param>
    /// <returns>Tuple of (attack, defense, speed) with all modifiers applied</returns>
    /// <remarks>
    /// Combines:
    /// - Base stats from Combat component
    /// - Equipment bonuses from EquipmentSlots (spec-001)
    /// - Status effect modifiers from StatusEffects
    /// 
    /// Enforces minimum values (attack/defense/speed >= 1).
    /// </remarks>
    public (int attack, int defense, int speed) CalculateTotalStats(Entity entity);

    // ═══════════════════════════════════════════════════════════════
    // QUERY OPERATIONS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets all active status effects on an entity.
    /// </summary>
    /// <param name="entity">The entity to query</param>
    /// <returns>List of active StatusEffect instances</returns>
    public List<StatusEffect> GetActiveEffects(Entity entity);

    /// <summary>
    /// Checks if an entity has a specific status effect active.
    /// </summary>
    /// <param name="entity">The entity to check</param>
    /// <param name="effectType">The effect type to check for</param>
    /// <returns>True if effect is active, false otherwise</returns>
    public bool HasEffect(Entity entity, EffectType effectType);

    /// <summary>
    /// Gets the remaining duration of a specific effect.
    /// </summary>
    /// <param name="entity">The entity to query</param>
    /// <param name="effectType">The effect type to check</param>
    /// <returns>Remaining duration in turns, or 0 if not active</returns>
    public int GetEffectDuration(Entity entity, EffectType effectType);

    /// <summary>
    /// Gets all entities currently affected by status effects.
    /// </summary>
    /// <returns>List of entities with active effects</returns>
    public List<Entity> GetAffectedEntities();

    /// <summary>
    /// Gets all DoT effects active on an entity.
    /// </summary>
    /// <param name="entity">The entity to query</param>
    /// <returns>List of damage-over-time effects</returns>
    public List<StatusEffect> GetDamageOverTimeEffects(Entity entity);

    /// <summary>
    /// Gets all HoT effects active on an entity.
    /// </summary>
    /// <param name="entity">The entity to query</param>
    /// <returns>List of healing-over-time effects</returns>
    public List<StatusEffect> GetHealingOverTimeEffects(Entity entity);

    /// <summary>
    /// Gets all buff effects active on an entity.
    /// </summary>
    /// <param name="entity">The entity to query</param>
    /// <returns>List of stat buff effects</returns>
    public List<StatusEffect> GetBuffEffects(Entity entity);

    /// <summary>
    /// Gets all debuff effects active on an entity.
    /// </summary>
    /// <param name="entity">The entity to query</param>
    /// <returns>List of stat debuff effects</returns>
    public List<StatusEffect> GetDebuffEffects(Entity entity);

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the effect definition for a given effect type.
    /// </summary>
    /// <param name="effectType">The effect type</param>
    /// <returns>Effect definition with default values</returns>
    public EffectDefinition GetEffectDefinition(EffectType effectType);

    /// <summary>
    /// Gets the display color for an effect type.
    /// </summary>
    /// <param name="effectType">The effect type</param>
    /// <returns>Color enum for HUD display</returns>
    public EffectColor GetEffectColor(EffectType effectType);

    /// <summary>
    /// Formats an effect for HUD display.
    /// </summary>
    /// <param name="effect">The status effect</param>
    /// <returns>Formatted string (e.g., "Poisoned (5)")</returns>
    public string FormatEffectForDisplay(StatusEffect effect);
}

/// <summary>
/// Result of a status effect operation.
/// </summary>
public struct StatusEffectResult
{
    /// <summary>Whether the operation succeeded</summary>
    public bool Success { get; set; }

    /// <summary>Feedback message to display to player</summary>
    public string Message { get; set; }

    /// <summary>The effect that was applied/removed (if applicable)</summary>
    public StatusEffect? Effect { get; set; }

    public static StatusEffectResult Succeeded(string message, StatusEffect? effect = null)
        => new() { Success = true, Message = message, Effect = effect };

    public static StatusEffectResult Failed(string message)
        => new() { Success = false, Message = message };
}

/// <summary>
/// Static class containing all effect definitions.
/// </summary>
public static class EffectDefinitions
{
    // Damage Over Time
    public static readonly EffectDefinition Poison = new()
    {
        Type = EffectType.Poison,
        DefaultMagnitude = 3,
        DefaultDuration = 5,
        Category = EffectCategory.DamageOverTime,
        Color = EffectColor.Red,
        Description = "Deals 3 damage per turn for 5 turns"
    };

    public static readonly EffectDefinition Bleed = new()
    {
        Type = EffectType.Bleed,
        DefaultMagnitude = 2,
        DefaultDuration = 8,
        Category = EffectCategory.DamageOverTime,
        Color = EffectColor.DarkRed,
        Description = "Deals 2 damage per turn for 8 turns"
    };

    public static readonly EffectDefinition Burning = new()
    {
        Type = EffectType.Burning,
        DefaultMagnitude = 4,
        DefaultDuration = 3,
        Category = EffectCategory.DamageOverTime,
        Color = EffectColor.Orange,
        Description = "Deals 4 damage per turn for 3 turns"
    };

    // Healing Over Time
    public static readonly EffectDefinition Regeneration = new()
    {
        Type = EffectType.Regeneration,
        DefaultMagnitude = 2,
        DefaultDuration = 10,
        Category = EffectCategory.HealingOverTime,
        Color = EffectColor.Green,
        Description = "Heals 2 HP per turn for 10 turns"
    };

    public static readonly EffectDefinition Blessed = new()
    {
        Type = EffectType.Blessed,
        DefaultMagnitude = 1,
        DefaultDuration = 20,
        Category = EffectCategory.HealingOverTime,
        Color = EffectColor.Yellow,
        Description = "Heals 1 HP per turn for 20 turns"
    };

    // Stat Buffs
    public static readonly EffectDefinition Strength = new()
    {
        Type = EffectType.Strength,
        DefaultMagnitude = 5,
        DefaultDuration = 10,
        Category = EffectCategory.StatBuff,
        Color = EffectColor.Green,
        Description = "Increases attack by 5 for 10 turns"
    };

    public static readonly EffectDefinition Haste = new()
    {
        Type = EffectType.Haste,
        DefaultMagnitude = 20,
        DefaultDuration = 8,
        Category = EffectCategory.StatBuff,
        Color = EffectColor.Cyan,
        Description = "Increases speed by 20 for 8 turns"
    };

    public static readonly EffectDefinition IronSkin = new()
    {
        Type = EffectType.IronSkin,
        DefaultMagnitude = 5,
        DefaultDuration = 12,
        Category = EffectCategory.StatBuff,
        Color = EffectColor.Blue,
        Description = "Increases defense by 5 for 12 turns"
    };

    // Stat Debuffs
    public static readonly EffectDefinition Weakness = new()
    {
        Type = EffectType.Weakness,
        DefaultMagnitude = 3,
        DefaultDuration = 6,
        Category = EffectCategory.StatDebuff,
        Color = EffectColor.Red,
        Description = "Decreases attack by 3 for 6 turns"
    };

    public static readonly EffectDefinition Slow = new()
    {
        Type = EffectType.Slow,
        DefaultMagnitude = 30,
        DefaultDuration = 6,
        Category = EffectCategory.StatDebuff,
        Color = EffectColor.Red,
        Description = "Decreases speed by 30 for 6 turns"
    };

    public static readonly EffectDefinition Fragile = new()
    {
        Type = EffectType.Fragile,
        DefaultMagnitude = 3,
        DefaultDuration = 6,
        Category = EffectCategory.StatDebuff,
        Color = EffectColor.DarkRed,
        Description = "Decreases defense by 3 for 6 turns"
    };

    /// <summary>
    /// Gets the definition for a given effect type.
    /// </summary>
    public static EffectDefinition GetDefinition(EffectType type)
    {
        return type switch
        {
            EffectType.Poison => Poison,
            EffectType.Bleed => Bleed,
            EffectType.Burning => Burning,
            EffectType.Regeneration => Regeneration,
            EffectType.Blessed => Blessed,
            EffectType.Strength => Strength,
            EffectType.Haste => Haste,
            EffectType.IronSkin => IronSkin,
            EffectType.Weakness => Weakness,
            EffectType.Slow => Slow,
            EffectType.Fragile => Fragile,
            _ => throw new ArgumentException($"Unknown effect type: {type}")
        };
    }
}
