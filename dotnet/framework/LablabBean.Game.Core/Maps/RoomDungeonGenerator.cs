using SadRogue.Primitives;

namespace LablabBean.Game.Core.Maps;

/// <summary>
/// Generates dungeons with rooms connected by corridors
/// </summary>
public class RoomDungeonGenerator
{
    private readonly Random _random;

    public RoomDungeonGenerator(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public record Room(Rectangle Bounds)
    {
        public Point Center => Bounds.Center;
        public bool Intersects(Room other) => Bounds.Intersects(other.Bounds);
    }

    /// <summary>
    /// Generates a dungeon with rooms and corridors
    /// </summary>
    public (DungeonMap Map, List<Room> Rooms) Generate(int width, int height, int maxRooms, int minRoomSize, int maxRoomSize)
    {
        var map = new DungeonMap(width, height);
        var rooms = new List<Room>();

        // Fill with walls initially
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var pos = new Point(x, y);
                map.SetWalkable(pos, false);
                map.SetTransparent(pos, false);
            }
        }

        // Try to place rooms
        for (int i = 0; i < maxRooms; i++)
        {
            int roomWidth = _random.Next(minRoomSize, maxRoomSize + 1);
            int roomHeight = _random.Next(minRoomSize, maxRoomSize + 1);
            int roomX = _random.Next(1, width - roomWidth - 1);
            int roomY = _random.Next(1, height - roomHeight - 1);

            var newRoom = new Room(new Rectangle(roomX, roomY, roomWidth, roomHeight));

            // Check if room intersects with existing rooms
            bool intersects = rooms.Any(r => r.Intersects(newRoom));

            if (!intersects)
            {
                // Carve out the room
                CarveRoom(map, newRoom);

                // Connect to previous room with corridor
                if (rooms.Count > 0)
                {
                    var prevRoom = rooms[^1];
                    ConnectRooms(map, prevRoom.Center, newRoom.Center);
                }

                rooms.Add(newRoom);
            }
        }

        return (map, rooms);
    }

    private void CarveRoom(DungeonMap map, Room room)
    {
        for (int x = room.Bounds.X; x < room.Bounds.X + room.Bounds.Width; x++)
        {
            for (int y = room.Bounds.Y; y < room.Bounds.Y + room.Bounds.Height; y++)
            {
                var pos = new Point(x, y);
                map.SetWalkable(pos, true);
                map.SetTransparent(pos, true);
            }
        }
    }

    private void ConnectRooms(DungeonMap map, Point start, Point end)
    {
        // Create L-shaped corridor
        var current = start;

        // Horizontal then vertical
        if (_random.Next(2) == 0)
        {
            // Move horizontally
            while (current.X != end.X)
            {
                map.SetWalkable(current, true);
                map.SetTransparent(current, true);
                current = new Point(current.X + Math.Sign(end.X - current.X), current.Y);
            }

            // Move vertically
            while (current.Y != end.Y)
            {
                map.SetWalkable(current, true);
                map.SetTransparent(current, true);
                current = new Point(current.X, current.Y + Math.Sign(end.Y - current.Y));
            }
        }
        else
        {
            // Move vertically
            while (current.Y != end.Y)
            {
                map.SetWalkable(current, true);
                map.SetTransparent(current, true);
                current = new Point(current.X, current.Y + Math.Sign(end.Y - current.Y));
            }

            // Move horizontally
            while (current.X != end.X)
            {
                map.SetWalkable(current, true);
                map.SetTransparent(current, true);
                current = new Point(current.X + Math.Sign(end.X - current.X), current.Y);
            }
        }

        // Make sure the end point is walkable
        map.SetWalkable(end, true);
        map.SetTransparent(end, true);
    }
}
