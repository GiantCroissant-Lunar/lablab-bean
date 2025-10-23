using SadRogue.Primitives;

namespace LablabBean.Game.Core.Maps;

/// <summary>
/// Tracks which tiles have been explored (fog of war)
/// </summary>
public class FogOfWar
{
    private readonly bool[,] _explored;

    public int Width { get; }
    public int Height { get; }

    public FogOfWar(int width, int height)
    {
        Width = width;
        Height = height;
        _explored = new bool[width, height];
    }

    /// <summary>
    /// Marks a tile as explored
    /// </summary>
    public void Explore(Point position)
    {
        if (IsInBounds(position))
        {
            _explored[position.X, position.Y] = true;
        }
    }

    /// <summary>
    /// Marks multiple tiles as explored
    /// </summary>
    public void Explore(IEnumerable<Point> positions)
    {
        foreach (var pos in positions)
        {
            Explore(pos);
        }
    }

    /// <summary>
    /// Checks if a tile has been explored
    /// </summary>
    public bool IsExplored(Point position)
    {
        return IsInBounds(position) && _explored[position.X, position.Y];
    }

    /// <summary>
    /// Gets all explored positions
    /// </summary>
    public IEnumerable<Point> GetExploredPositions()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (_explored[x, y])
                {
                    yield return new Point(x, y);
                }
            }
        }
    }

    /// <summary>
    /// Clears all explored tiles
    /// </summary>
    public void Clear()
    {
        Array.Clear(_explored, 0, _explored.Length);
    }

    private bool IsInBounds(Point position)
    {
        return position.X >= 0 && position.X < Width &&
               position.Y >= 0 && position.Y < Height;
    }
}
