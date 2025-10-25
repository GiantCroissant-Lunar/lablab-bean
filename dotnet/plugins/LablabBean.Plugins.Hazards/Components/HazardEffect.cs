namespace LablabBean.Plugins.Hazards.Components;

/// <summary>
/// Ongoing effect from a hazard (e.g., burning, poisoned)
/// </summary>
public struct HazardEffect
{
    public HazardType SourceType;
    public int DamagePerTurn;
    public int RemainingTurns;
    public string EffectName;

    public HazardEffect(HazardType sourceType, int damagePerTurn, int duration, string effectName)
    {
        SourceType = sourceType;
        DamagePerTurn = damagePerTurn;
        RemainingTurns = duration;
        EffectName = effectName;
    }
}
