using MessagePack;

namespace LablabBean.Contracts.Serialization.Examples;

/// <summary>
/// Example game event using MessagePack attributes for optimal performance
/// </summary>
[MessagePackObject]
public class GameEvent
{
    [Key(0)]
    public string EventType { get; set; } = string.Empty;

    [Key(1)]
    public DateTime Timestamp { get; set; }

    [Key(2)]
    public string PlayerId { get; set; } = string.Empty;

    [Key(3)]
    public Dictionary<string, object> Data { get; set; } = new();

    [Key(4)]
    public EventSeverity Severity { get; set; } = EventSeverity.Info;
}

/// <summary>
/// Example analytics data model for batch processing
/// </summary>
[MessagePackObject]
public class AnalyticsData
{
    [Key(0)]
    public string SessionId { get; set; } = string.Empty;

    [Key(1)]
    public List<PlayerAction> Actions { get; set; } = new();

    [Key(2)]
    public PlayerStats Stats { get; set; } = new();

    [Key(3)]
    public TimeSpan SessionDuration { get; set; }

    [Key(4)]
    public Dictionary<string, double> Metrics { get; set; } = new();
}

[MessagePackObject]
public class PlayerAction
{
    [Key(0)]
    public string ActionType { get; set; } = string.Empty;

    [Key(1)]
    public DateTime Timestamp { get; set; }

    [Key(2)]
    public Vector3 Position { get; set; }

    [Key(3)]
    public string? TargetId { get; set; }

    [Key(4)]
    public double Value { get; set; }
}

[MessagePackObject]
public class PlayerStats
{
    [Key(0)]
    public int Level { get; set; }

    [Key(1)]
    public long Experience { get; set; }

    [Key(2)]
    public int Health { get; set; }

    [Key(3)]
    public int MaxHealth { get; set; }

    [Key(4)]
    public Dictionary<string, int> Attributes { get; set; } = new();
}

[MessagePackObject]
public struct Vector3
{
    [Key(0)]
    public float X { get; set; }

    [Key(1)]
    public float Y { get; set; }

    [Key(2)]
    public float Z { get; set; }

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}

public enum EventSeverity
{
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3,
    Critical = 4
}

/// <summary>
/// Example session state for persistence
/// </summary>
[MessagePackObject]
public class SessionState
{
    [Key(0)]
    public string PlayerId { get; set; } = string.Empty;

    [Key(1)]
    public PlayerStats PlayerStats { get; set; } = new();

    [Key(2)]
    public List<InventoryItem> Inventory { get; set; } = new();

    [Key(3)]
    public WorldState WorldState { get; set; } = new();

    [Key(4)]
    public Dictionary<string, bool> Achievements { get; set; } = new();

    [Key(5)]
    public DateTime LastSaved { get; set; }
}

[MessagePackObject]
public class InventoryItem
{
    [Key(0)]
    public string ItemId { get; set; } = string.Empty;

    [Key(1)]
    public int Quantity { get; set; }

    [Key(2)]
    public Dictionary<string, object> Properties { get; set; } = new();
}

[MessagePackObject]
public class WorldState
{
    [Key(0)]
    public int CurrentLevel { get; set; }

    [Key(1)]
    public Vector3 PlayerPosition { get; set; }

    [Key(2)]
    public List<string> CompletedQuests { get; set; } = new();

    [Key(3)]
    public Dictionary<string, object> Variables { get; set; } = new();
}
