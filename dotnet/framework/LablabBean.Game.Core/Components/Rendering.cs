using SadRogue.Primitives;

namespace LablabBean.Game.Core.Components;

/// <summary>
/// Visual representation of an entity
/// Uses a single character/glyph for rendering in both Terminal.Gui and SadConsole
/// </summary>
public struct Renderable
{
    /// <summary>
    /// The character/glyph to display
    /// </summary>
    public char Glyph { get; set; }

    /// <summary>
    /// Foreground color
    /// </summary>
    public Color Foreground { get; set; }

    /// <summary>
    /// Background color
    /// </summary>
    public Color Background { get; set; }

    /// <summary>
    /// Z-order for rendering (higher = drawn on top)
    /// </summary>
    public int ZOrder { get; set; }

    public Renderable(char glyph, Color foreground, Color? background = null, int zOrder = 0)
    {
        Glyph = glyph;
        Foreground = foreground;
        Background = background ?? Color.Black;
        ZOrder = zOrder;
    }
}

/// <summary>
/// Component for entities that can be seen
/// </summary>
public struct Visible
{
    public bool IsVisible { get; set; }

    public Visible(bool isVisible = true)
    {
        IsVisible = isVisible;
    }
}

/// <summary>
/// Component for entities that block vision (walls, etc.)
/// </summary>
public struct BlocksVision
{
    public bool Blocks { get; set; }

    public BlocksVision(bool blocks = true)
    {
        Blocks = blocks;
    }
}
