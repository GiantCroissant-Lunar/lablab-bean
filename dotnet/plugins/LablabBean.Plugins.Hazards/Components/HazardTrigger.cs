namespace LablabBean.Plugins.Hazards.Components;

/// <summary>
/// Defines how a hazard is triggered
/// </summary>
public enum TriggerType
{
    OnEnter,        // Triggers when entity enters tile
    OnExit,         // Triggers when entity exits tile
    Periodic,       // Triggers every N turns
    Proximity,      // Triggers when entity is nearby
    Manual          // Requires manual activation
}

/// <summary>
/// Configuration for hazard triggering
/// </summary>
public struct HazardTrigger
{
    public TriggerType TriggerType;
    public int Period;          // For Periodic triggers
    public int ProximityRange;  // For Proximity triggers
    public int TurnsSinceLastTrigger;
    public bool CanRetrigger;
    public int RetriggerDelay;

    public HazardTrigger(TriggerType triggerType, bool canRetrigger = true, int retriggerDelay = 0)
    {
        TriggerType = triggerType;
        Period = 0;
        ProximityRange = 0;
        TurnsSinceLastTrigger = 0;
        CanRetrigger = canRetrigger;
        RetriggerDelay = retriggerDelay;
    }
}
