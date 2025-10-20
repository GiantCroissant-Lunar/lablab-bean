using SadRogue.Primitives;

namespace LablabBean.Game.Core.Components;

/// <summary>
/// Position component using GoRogue's Point structure
/// </summary>
public struct Position
{
    public Point Point { get; set; }

    public Position(int x, int y)
    {
        Point = new Point(x, y);
    }

    public Position(Point point)
    {
        Point = point;
    }

    public int X => Point.X;
    public int Y => Point.Y;
}

/// <summary>
/// Direction component for facing direction
/// </summary>
public struct Direction
{
    public SadRogue.Primitives.Direction Value { get; set; }

    public Direction(SadRogue.Primitives.Direction direction)
    {
        Value = direction;
    }
}

/// <summary>
/// Velocity component for movement
/// </summary>
public struct Velocity
{
    public Point Delta { get; set; }

    public Velocity(int dx, int dy)
    {
        Delta = new Point(dx, dy);
    }
}
