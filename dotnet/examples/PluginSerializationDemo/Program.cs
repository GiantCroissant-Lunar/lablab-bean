using LablabBean.Contracts.Serialization.Services;
using LablabBean.Contracts.Serialization.Examples;
using LablabBean.Contracts.Serialization.Configuration;
using LablabBean.Contracts.Serialization;
using LablabBean.Plugins.Contracts;

namespace PluginSerializationDemo;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Plugin Serialization Integration Demo");
        Console.WriteLine("====================================");

        // Simulate plugin registry
        var registry = new SimpleRegistry();

        // Register different serialization services
        registry.Register<IService>(new MessagePackSerializationService(MessagePackOptions.CreateHighPerformance()));

        // Create a mock plugin that uses serialization
        var analyticsPlugin = new AnalyticsPlugin(registry);
        var eventBusPlugin = new EventBusPlugin(registry);

        Console.WriteLine("\n1. Analytics Plugin Demo");
        await DemoAnalyticsPlugin(analyticsPlugin);

        Console.WriteLine("\n2. Event Bus Plugin Demo");
        await DemoEventBusPlugin(eventBusPlugin);

        Console.WriteLine("\n3. Session Persistence Demo");
        await DemoSessionPersistence(registry);

        Console.WriteLine("\nPlugin integration demo completed!");
    }

    static async Task DemoAnalyticsPlugin(AnalyticsPlugin plugin)
    {
        // Simulate collecting analytics data
        var sessionData = new AnalyticsData
        {
            SessionId = Guid.NewGuid().ToString(),
            Actions = new List<PlayerAction>
            {
                new() { ActionType = "login", Timestamp = DateTime.UtcNow, Position = new Vector3(0, 0, 0), Value = 1 },
                new() { ActionType = "move", Timestamp = DateTime.UtcNow.AddSeconds(1), Position = new Vector3(10, 0, 5), Value = 5.5 },
                new() { ActionType = "attack", Timestamp = DateTime.UtcNow.AddSeconds(2), Position = new Vector3(12, 0, 7), TargetId = "enemy_001", Value = 25 },
                new() { ActionType = "logout", Timestamp = DateTime.UtcNow.AddSeconds(60), Position = new Vector3(15, 0, 10), Value = 1 }
            },
            Stats = new PlayerStats { Level = 5, Experience = 1250, Health = 80, MaxHealth = 100 },
            SessionDuration = TimeSpan.FromMinutes(1),
            Metrics = new Dictionary<string, double>
            {
                ["damage_dealt"] = 25,
                ["distance_traveled"] = 15.8,
                ["session_score"] = 150
            }
        };

        await plugin.RecordSessionAsync(sessionData);
        var report = await plugin.GenerateReportAsync();

        Console.WriteLine($"✓ Recorded session: {sessionData.SessionId}");
        Console.WriteLine($"✓ Generated report: {report.Length} bytes");
    }

    static async Task DemoEventBusPlugin(EventBusPlugin plugin)
    {
        var events = new List<GameEvent>
        {
            new() { EventType = "PlayerJoined", PlayerId = "player_001", Timestamp = DateTime.UtcNow, Severity = EventSeverity.Info },
            new() { EventType = "ItemPickup", PlayerId = "player_001", Timestamp = DateTime.UtcNow.AddSeconds(5), Severity = EventSeverity.Info, Data = new() { ["item"] = "sword", ["quantity"] = 1 } },
            new() { EventType = "EnemyDefeated", PlayerId = "player_001", Timestamp = DateTime.UtcNow.AddSeconds(30), Severity = EventSeverity.Info, Data = new() { ["enemy"] = "goblin", ["xp"] = 50 } }
        };

        foreach (var gameEvent in events)
        {
            await plugin.PublishEventAsync(gameEvent);
        }

        var serializedEvents = await plugin.GetEventHistoryAsync();
        Console.WriteLine($"✓ Published {events.Count} events");
        Console.WriteLine($"✓ Event history: {serializedEvents.Length} bytes total");
    }
    static async Task DemoSessionPersistence(SimpleRegistry registry)
    {
        var serializer = registry.Get<IService>();

        var sessionState = new SessionState
        {
            PlayerId = "demo_player",
            PlayerStats = new PlayerStats { Level = 10, Experience = 5000, Health = 100, MaxHealth = 100 },
            Inventory = new List<InventoryItem>
            {
                new() { ItemId = "sword", Quantity = 1 },
                new() { ItemId = "potion", Quantity = 3 },
                new() { ItemId = "gold", Quantity = 500 }
            },
            WorldState = new WorldState
            {
                CurrentLevel = 2,
                PlayerPosition = new Vector3(25, 0, 30),
                CompletedQuests = new List<string> { "tutorial", "first_quest" }
            },
            LastSaved = DateTime.UtcNow
        };

        // Save session
        var saveData = await serializer.SerializeAsync(sessionState, SerializationFormat.MessagePack, CancellationToken.None);

        // Simulate loading session
        var loadedSession = await serializer.DeserializeAsync<SessionState>(saveData, SerializationFormat.MessagePack, CancellationToken.None);

        Console.WriteLine($"✓ Saved session: {saveData.Length} bytes");
        Console.WriteLine($"✓ Loaded player: {loadedSession.PlayerId} (Level {loadedSession.PlayerStats.Level})");
        Console.WriteLine($"✓ Inventory items: {loadedSession.Inventory.Count}");
        Console.WriteLine($"✓ Current level: {loadedSession.WorldState.CurrentLevel}");
    }
}

// Mock plugin implementations
public class AnalyticsPlugin
{
    private readonly IRegistry _registry;
    private readonly List<AnalyticsData> _sessions = new();

    public AnalyticsPlugin(IRegistry registry)
    {
        _registry = registry;
    }

    public async Task RecordSessionAsync(AnalyticsData session)
    {
        _sessions.Add(session);

        // Serialize for storage/transmission
        var serializer = _registry.Get<IService>();
        var data = await serializer.SerializeAsync(session, SerializationFormat.MessagePack, CancellationToken.None);

        // Simulate storing to database/file
        Console.WriteLine($"  Stored session data: {data.Length} bytes");
    }

    public async Task<byte[]> GenerateReportAsync()
    {
        var serializer = _registry.Get<IService>();
        return await serializer.SerializeAsync(_sessions, SerializationFormat.MessagePack, CancellationToken.None);
    }
}

public class EventBusPlugin
{
    private readonly IRegistry _registry;
    private readonly List<GameEvent> _eventHistory = new();

    public EventBusPlugin(IRegistry registry)
    {
        _registry = registry;
    }

    public async Task PublishEventAsync(GameEvent gameEvent)
    {
        _eventHistory.Add(gameEvent);

        // Serialize for event transmission
        var serializer = _registry.Get<IService>();
        var data = await serializer.SerializeAsync(gameEvent, SerializationFormat.MessagePack, CancellationToken.None);

        Console.WriteLine($"  Published {gameEvent.EventType}: {data.Length} bytes");
    }

    public async Task<byte[]> GetEventHistoryAsync()
    {
        var serializer = _registry.Get<IService>();
        return await serializer.SerializeAsync(_eventHistory, SerializationFormat.MessagePack, CancellationToken.None);
    }
}

// Simple registry implementation for demo
public class SimpleRegistry : IRegistry
{
    private readonly Dictionary<Type, List<object>> _services = new();

    public void Register<T>(T service) where T : class
    {
        var type = typeof(T);
        if (!_services.ContainsKey(type))
            _services[type] = new List<object>();

        _services[type].Add(service);
    }

    public T Get<T>(SelectionMode mode = SelectionMode.HighestPriority) where T : class
    {
        var type = typeof(T);
        if (_services.TryGetValue(type, out var services) && services.Count > 0)
        {
            return (T)services[0];
        }
        throw new InvalidOperationException($"No service registered for type {type.Name}");
    }

    public IEnumerable<T> GetAll<T>() where T : class
    {
        var type = typeof(T);
        if (_services.TryGetValue(type, out var services))
        {
            return services.Cast<T>();
        }
        return Enumerable.Empty<T>();
    }

    public bool IsRegistered<T>() where T : class
    {
        return _services.ContainsKey(typeof(T));
    }

    public void Unregister<T>(T service) where T : class
    {
        var type = typeof(T);
        if (_services.TryGetValue(type, out var services))
        {
            services.Remove(service);
        }
    }
}
