using LablabBean.Plugins.Hazards.Components;

namespace LablabBean.Plugins.Hazards.Data;

/// <summary>
/// Definition of a hazard template
/// </summary>
public class HazardDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public HazardType Type { get; set; }
    public int Damage { get; set; }
    public float ActivationChance { get; set; } = 1.0f;
    public bool IsVisible { get; set; } = true;
    public bool RequiresDetection { get; set; }
    public int DetectionDifficulty { get; set; } = 10;
    public TriggerType TriggerType { get; set; } = TriggerType.OnEnter;
    public int TriggerPeriod { get; set; }
    public int ProximityRange { get; set; }
    public bool CanRetrigger { get; set; } = true;
    public int RetriggerDelay { get; set; }
    public char Glyph { get; set; } = '^';
    public string Color { get; set; } = "Red";
}
