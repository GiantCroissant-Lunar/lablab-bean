namespace LablabBean.AI.Core.Events;

/// <summary>
/// Event fired when player behavior is observed during combat
/// Used for tactical learning and adaptation
/// </summary>
public class PlayerBehaviorObservedEvent
{
    public string PlayerId { get; set; } = string.Empty;
    public PlayerBehaviorType BehaviorType { get; set; }
    public float Intensity { get; set; }
    public DateTime ObservedAt { get; set; }
    public string Context { get; set; } = string.Empty;

    public PlayerBehaviorObservedEvent() { }

    public PlayerBehaviorObservedEvent(
        string playerId,
        PlayerBehaviorType behaviorType,
        float intensity,
        string context = "")
    {
        PlayerId = playerId;
        BehaviorType = behaviorType;
        Intensity = intensity;
        ObservedAt = DateTime.UtcNow;
        Context = context;
    }
}

/// <summary>
/// Types of observable player behaviors
/// </summary>
public enum PlayerBehaviorType
{
    Unknown,
    RangedAttacks,      // Player prefers distance, projectiles
    MeleeAggressive,    // Player rushes in with melee
    HitAndRun,          // Player attacks then retreats
    Defensive,          // Player blocks, dodges, stays cautious
    HealingFocused,     // Player frequently heals
    AreaOfEffect,       // Player uses AOE attacks
    StatusEffects,      // Player uses debuffs, poisons, etc.
    Kiting              // Player maintains distance while attacking
}
