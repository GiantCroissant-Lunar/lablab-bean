namespace LablabBean.Game.Core.Components;

/// <summary>
/// Directions for staircase movement
/// </summary>
public enum StaircaseDirection
{
    Up,
    Down
}

/// <summary>
/// Component for staircase entities that enable level transitions
/// </summary>
public struct Staircase
{
    public StaircaseDirection Direction { get; set; }
    public int TargetLevel { get; set; }

    public Staircase(StaircaseDirection direction, int targetLevel)
    {
        Direction = direction;
        TargetLevel = targetLevel;
    }

    public char Glyph => Direction == StaircaseDirection.Down ? '>' : '<';
    public string Description => Direction == StaircaseDirection.Down
        ? $"Descend to Level {TargetLevel}"
        : $"Ascend to Level {TargetLevel}";
}
