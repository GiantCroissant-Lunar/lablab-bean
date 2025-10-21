using LablabBean.Contracts.Game.Models;

namespace LablabBean.Contracts.UI.Models;

/// <summary>
/// Viewport bounds (visible area).
/// </summary>
public record ViewportBounds(
    Position TopLeft,
    int Width,
    int Height
)
{
    public Position Center => new(TopLeft.X + Width / 2, TopLeft.Y + Height / 2);
    public Position BottomRight => new(TopLeft.X + Width - 1, TopLeft.Y + Height - 1);
}
