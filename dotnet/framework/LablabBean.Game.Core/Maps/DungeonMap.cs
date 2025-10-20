using GoRogue.FOV;
using GoRogue.MapGeneration;
using GoRogue.Pathing;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace LablabBean.Game.Core.Maps;

/// <summary>
/// Represents a dungeon map with FOV and pathfinding capabilities
/// Uses GoRogue for roguelike features
/// </summary>
public class DungeonMap
{
    private readonly ArrayView<bool> _walkabilityMap;
    private readonly ArrayView<bool> _transparencyMap;
    private readonly IFOV _fov;
    private readonly AStar _pathfinder;

    public int Width { get; }
    public int Height { get; }

    /// <summary>
    /// Field of view instance
    /// </summary>
    public IFOV FOV => _fov;

    /// <summary>
    /// Pathfinding instance
    /// </summary>
    public AStar Pathfinder => _pathfinder;

    public DungeonMap(int width, int height)
    {
        Width = width;
        Height = height;

        // Initialize walkability and transparency maps
        _walkabilityMap = new ArrayView<bool>(width, height);
        _transparencyMap = new ArrayView<bool>(width, height);

        // Initialize FOV with the transparency map
        _fov = new RecursiveShadowcastingFOV(_transparencyMap);

        // Initialize pathfinder with the walkability map
        _pathfinder = new AStar(_walkabilityMap, Distance.Chebyshev);
    }

    /// <summary>
    /// Sets whether a tile is walkable
    /// </summary>
    public void SetWalkable(Point position, bool walkable)
    {
        if (!IsInBounds(position)) return;
        _walkabilityMap[position] = walkable;
    }

    /// <summary>
    /// Sets whether a tile is transparent (for FOV)
    /// </summary>
    public void SetTransparent(Point position, bool transparent)
    {
        if (!IsInBounds(position)) return;
        _transparencyMap[position] = transparent;
    }

    /// <summary>
    /// Checks if a position is walkable
    /// </summary>
    public bool IsWalkable(Point position)
    {
        return IsInBounds(position) && _walkabilityMap[position];
    }

    /// <summary>
    /// Checks if a position is transparent
    /// </summary>
    public bool IsTransparent(Point position)
    {
        return IsInBounds(position) && _transparencyMap[position];
    }

    /// <summary>
    /// Checks if a position is within map bounds
    /// </summary>
    public bool IsInBounds(Point position)
    {
        return position.X >= 0 && position.X < Width && position.Y >= 0 && position.Y < Height;
    }

    /// <summary>
    /// Calculates FOV from a position
    /// </summary>
    public void CalculateFOV(Point origin, int radius)
    {
        _fov.Calculate(origin, radius);
    }

    /// <summary>
    /// Checks if a position is in the current FOV
    /// </summary>
    public bool IsInFOV(Point position)
    {
        return IsInBounds(position) && _fov.BooleanResultView[position];
    }

    /// <summary>
    /// Gets all positions currently in FOV
    /// </summary>
    public IEnumerable<Point> GetVisiblePositions()
    {
        return _fov.CurrentFOV;
    }

    /// <summary>
    /// Finds a path between two points
    /// </summary>
    public GoRogue.Pathing.Path? FindPath(Point start, Point end)
    {
        return _pathfinder.ShortestPath(start, end);
    }

    /// <summary>
    /// Gets the distance between two points
    /// </summary>
    public double GetDistance(Point start, Point end)
    {
        return Distance.Chebyshev.Calculate(start, end);
    }

    /// <summary>
    /// Gets all walkable neighbors of a position
    /// </summary>
    public IEnumerable<Point> GetWalkableNeighbors(Point position)
    {
        foreach (var direction in AdjacencyRule.EightWay.DirectionsOfNeighbors())
        {
            var neighbor = position + direction;
            if (IsWalkable(neighbor))
                yield return neighbor;
        }
    }

    /// <summary>
    /// Fills the entire map with floor tiles (walkable and transparent)
    /// </summary>
    public void FillWithFloor()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var pos = new Point(x, y);
                SetWalkable(pos, true);
                SetTransparent(pos, true);
            }
        }
    }

    /// <summary>
    /// Creates walls around the perimeter of the map
    /// </summary>
    public void CreatePerimeterWalls()
    {
        for (int x = 0; x < Width; x++)
        {
            SetWalkable(new Point(x, 0), false);
            SetTransparent(new Point(x, 0), false);
            SetWalkable(new Point(x, Height - 1), false);
            SetTransparent(new Point(x, Height - 1), false);
        }

        for (int y = 0; y < Height; y++)
        {
            SetWalkable(new Point(0, y), false);
            SetTransparent(new Point(0, y), false);
            SetWalkable(new Point(Width - 1, y), false);
            SetTransparent(new Point(Width - 1, y), false);
        }
    }
}
