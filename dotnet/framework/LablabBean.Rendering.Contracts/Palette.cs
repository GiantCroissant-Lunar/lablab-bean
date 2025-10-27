namespace LablabBean.Rendering.Contracts;

/// <summary>
/// Optional palette description (for terminal color or tileset tinting).
/// </summary>
public sealed class Palette
{
    public IReadOnlyList<uint> ArgbColors { get; }

    public Palette(IReadOnlyList<uint> argbColors)
    {
        ArgbColors = argbColors;
    }
}
