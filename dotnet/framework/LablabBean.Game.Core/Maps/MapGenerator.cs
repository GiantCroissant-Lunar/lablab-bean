using SadRogue.Primitives;

namespace LablabBean.Game.Core.Maps;

/// <summary>
/// Generates dungeon maps using various algorithms
/// </summary>
public class MapGenerator
{
    private readonly Random _random;

    public MapGenerator(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    /// <summary>
    /// Generates a simple rectangular room-based dungeon
    /// </summary>
    public DungeonMap GenerateRoomsAndCorridors(int width, int height, int minRoomSize = 4, int maxRoomSize = 10, int maxRooms = 30)
    {
        var map = new DungeonMap(width, height);

        // Start with all walls
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map.SetWalkable(new Point(x, y), false);
                map.SetTransparent(new Point(x, y), false);
            }
        }

        var rooms = new List<Rectangle>();

        for (int i = 0; i < maxRooms; i++)
        {
            // Random room dimensions
            int roomWidth = _random.Next(minRoomSize, maxRoomSize + 1);
            int roomHeight = _random.Next(minRoomSize, maxRoomSize + 1);

            // Random position within map bounds
            int x = _random.Next(1, width - roomWidth - 1);
            int y = _random.Next(1, height - roomHeight - 1);

            var newRoom = new Rectangle(x, y, roomWidth, roomHeight);

            // Check if room intersects with existing rooms
            bool intersects = rooms.Any(room => room.Intersects(newRoom));

            if (!intersects)
            {
                // Create the room
                CreateRoom(map, newRoom);

                // Connect to previous room with a corridor
                if (rooms.Count > 0)
                {
                    var prevRoom = rooms[^1];
                    CreateCorridor(map, prevRoom.Center, newRoom.Center);
                }

                rooms.Add(newRoom);
            }
        }

        return map;
    }

    /// <summary>
    /// Generates a cellular automata cave-like dungeon
    /// </summary>
    public DungeonMap GenerateCave(int width, int height, int iterations = 4, float wallProbability = 0.45f)
    {
        var map = new DungeonMap(width, height);

        // Initialize with random walls
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var pos = new Point(x, y);
                bool isWall = _random.NextDouble() < wallProbability;
                map.SetWalkable(pos, !isWall);
                map.SetTransparent(pos, !isWall);
            }
        }

        // Apply cellular automata
        for (int i = 0; i < iterations; i++)
        {
            map = ApplyCellularAutomata(map);
        }

        return map;
    }

    /// <summary>
    /// Generates a simple test map with a single room
    /// </summary>
    public DungeonMap GenerateSimpleTestMap(int width, int height)
    {
        var map = new DungeonMap(width, height);
        map.FillWithFloor();
        map.CreatePerimeterWalls();

        // Add some obstacles in the middle
        var centerRoom = new Rectangle(width / 4, height / 4, width / 2, height / 2);
        for (int x = centerRoom.X; x < centerRoom.MaxExtentX; x++)
        {
            map.SetWalkable(new Point(x, centerRoom.Y), false);
            map.SetTransparent(new Point(x, centerRoom.Y), false);
            map.SetWalkable(new Point(x, centerRoom.MaxExtentY - 1), false);
            map.SetTransparent(new Point(x, centerRoom.MaxExtentY - 1), false);
        }

        for (int y = centerRoom.Y; y < centerRoom.MaxExtentY; y++)
        {
            map.SetWalkable(new Point(centerRoom.X, y), false);
            map.SetTransparent(new Point(centerRoom.X, y), false);
            map.SetWalkable(new Point(centerRoom.MaxExtentX - 1, y), false);
            map.SetTransparent(new Point(centerRoom.MaxExtentX - 1, y), false);
        }

        return map;
    }

    private void CreateRoom(DungeonMap map, Rectangle room)
    {
        for (int x = room.X; x < room.MaxExtentX; x++)
        {
            for (int y = room.Y; y < room.MaxExtentY; y++)
            {
                var pos = new Point(x, y);
                map.SetWalkable(pos, true);
                map.SetTransparent(pos, true);
            }
        }
    }

    private void CreateCorridor(DungeonMap map, Point start, Point end)
    {
        // Create L-shaped corridor
        var current = start;

        // Horizontal first
        while (current.X != end.X)
        {
            map.SetWalkable(current, true);
            map.SetTransparent(current, true);
            current = current.X < end.X
                ? new Point(current.X + 1, current.Y)
                : new Point(current.X - 1, current.Y);
        }

        // Then vertical
        while (current.Y != end.Y)
        {
            map.SetWalkable(current, true);
            map.SetTransparent(current, true);
            current = current.Y < end.Y
                ? new Point(current.X, current.Y + 1)
                : new Point(current.X, current.Y - 1);
        }

        // Make sure the end point is walkable
        map.SetWalkable(end, true);
        map.SetTransparent(end, true);
    }

    private DungeonMap ApplyCellularAutomata(DungeonMap oldMap)
    {
        var newMap = new DungeonMap(oldMap.Width, oldMap.Height);

        for (int x = 0; x < oldMap.Width; x++)
        {
            for (int y = 0; y < oldMap.Height; y++)
            {
                var pos = new Point(x, y);
                int wallCount = CountAdjacentWalls(oldMap, pos);

                // Apply rules: if 5 or more neighbors are walls, become a wall
                bool isWall = wallCount >= 5;
                newMap.SetWalkable(pos, !isWall);
                newMap.SetTransparent(pos, !isWall);
            }
        }

        return newMap;
    }

    private int CountAdjacentWalls(DungeonMap map, Point position)
    {
        int count = 0;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                var neighbor = new Point(position.X + dx, position.Y + dy);

                // Treat out of bounds as walls
                if (!map.IsInBounds(neighbor) || !map.IsWalkable(neighbor))
                {
                    count++;
                }
            }
        }

        return count;
    }
}
