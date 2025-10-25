namespace LablabBean.Plugins.Hazards.Components;

/// <summary>
/// Resistance to specific hazard types
/// </summary>
public struct HazardResistance
{
    public Dictionary<HazardType, float> Resistances;

    public HazardResistance()
    {
        Resistances = new Dictionary<HazardType, float>();
    }

    public float GetResistance(HazardType type)
    {
        return Resistances.TryGetValue(type, out var resistance) ? resistance : 0f;
    }

    public void SetResistance(HazardType type, float value)
    {
        Resistances[type] = Math.Clamp(value, 0f, 1f);
    }
}
