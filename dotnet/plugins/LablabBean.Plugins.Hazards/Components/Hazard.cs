namespace LablabBean.Plugins.Hazards.Components;

/// <summary>
/// Marks an entity as an environmental hazard
/// </summary>
public struct Hazard
{
    public HazardType Type;
    public HazardState State;
    public int Damage;
    public float ActivationChance;
    public bool IsVisible;
    public bool RequiresDetection;
    public int DetectionDifficulty;

    public Hazard(
        HazardType type,
        int damage,
        float activationChance = 1.0f,
        bool isVisible = true,
        bool requiresDetection = false,
        int detectionDifficulty = 10)
    {
        Type = type;
        State = HazardState.Active;
        Damage = damage;
        ActivationChance = activationChance;
        IsVisible = isVisible;
        RequiresDetection = requiresDetection;
        DetectionDifficulty = detectionDifficulty;
    }
}
