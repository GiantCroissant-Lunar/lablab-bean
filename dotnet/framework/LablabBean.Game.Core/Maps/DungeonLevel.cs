using Arch.Core;
using SadRogue.Primitives;

namespace LablabBean.Game.Core.Maps;

/// <summary>
/// Represents a complete snapshot of a dungeon level
/// </summary>
public class DungeonLevel
{
    public int LevelNumber { get; set; }
    public DungeonMap Map { get; set; }
    public List<EntitySnapshot> Entities { get; set; }
    public Point UpStaircasePosition { get; set; }
    public Point DownStaircasePosition { get; set; }
    public DateTime LastVisited { get; set; }

    public DungeonLevel(int levelNumber, DungeonMap map)
    {
        LevelNumber = levelNumber;
        Map = map;
        Entities = new List<EntitySnapshot>();
        LastVisited = DateTime.UtcNow;
    }
}

/// <summary>
/// Snapshot of an entity for level state persistence
/// </summary>
public class EntitySnapshot
{
    public string Archetype { get; set; }
    public Dictionary<string, object> Components { get; set; }
    public bool IsActive { get; set; }

    public EntitySnapshot()
    {
        Archetype = string.Empty;
        Components = new Dictionary<string, object>();
        IsActive = true;
    }
}
