using System.Diagnostics;
using LablabBean.Contracts.Serialization.Services;
using LablabBean.Contracts.Serialization.Examples;
using LablabBean.Contracts.Serialization.Configuration;
using LablabBean.Contracts.Serialization;

namespace MessagePackIntegrationDemo;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("MessagePack Integration Demo");
        Console.WriteLine("===========================");

        await DemoBasicUsage();
        await DemoPerformanceConfigurations();
        await DemoRealWorldScenarios();

        Console.WriteLine("\nIntegration demo completed successfully!");
    }

    static async Task DemoBasicUsage()
    {
        Console.WriteLine("\n1. Basic MessagePack Usage");

        var service = new MessagePackSerializationService();

        var gameEvent = new GameEvent
        {
            EventType = "PlayerMove",
            PlayerId = "player_123",
            Timestamp = DateTime.UtcNow,
            Severity = EventSeverity.Info,
            Data = new Dictionary<string, object>
            {
                ["from"] = new Vector3(10, 0, 15),
                ["to"] = new Vector3(12, 0, 17),
                ["speed"] = 5.5
            }
        };

        // Test all serialization methods
        var bytes = await service.SerializeAsync(gameEvent, SerializationFormat.MessagePack, CancellationToken.None);
        var base64 = await service.SerializeToStringAsync(gameEvent, SerializationFormat.MessagePack, CancellationToken.None);

        using var stream = new MemoryStream();
        await service.SerializeToStreamAsync(gameEvent, stream, SerializationFormat.MessagePack, CancellationToken.None);

        Console.WriteLine($"✓ Byte serialization: {bytes.Length} bytes");
        Console.WriteLine($"✓ Base64 serialization: {base64.Length} characters");
        Console.WriteLine($"✓ Stream serialization: {stream.Length} bytes");

        // Test deserialization
        var fromBytes = await service.DeserializeAsync<GameEvent>(bytes, SerializationFormat.MessagePack, CancellationToken.None);
        var fromBase64 = await service.DeserializeFromStringAsync<GameEvent>(base64, SerializationFormat.MessagePack, CancellationToken.None);

        stream.Position = 0;
        var fromStream = await service.DeserializeFromStreamAsync<GameEvent>(stream, SerializationFormat.MessagePack, CancellationToken.None);

        Console.WriteLine($"✓ All deserializations successful: {fromBytes.EventType}, {fromBase64.EventType}, {fromStream.EventType}");
    }

    static async Task DemoPerformanceConfigurations()
    {
        Console.WriteLine("\n2. Performance Configuration Comparison");

        var testData = CreateLargeAnalyticsData();

        // Test different configurations
        var defaultService = new MessagePackSerializationService(MessagePackOptions.CreateDefault());
        var highPerfService = new MessagePackSerializationService(MessagePackOptions.CreateHighPerformance());
        var compactService = new MessagePackSerializationService(MessagePackOptions.CreateCompact());

        await BenchmarkConfiguration("Default", defaultService, testData);
        await BenchmarkConfiguration("High Performance", highPerfService, testData);
        await BenchmarkConfiguration("Compact", compactService, testData);
    }

    static async Task BenchmarkConfiguration(string name, MessagePackSerializationService service, AnalyticsData data)
    {
        const int iterations = 100;

        var sw = Stopwatch.StartNew();
        byte[] lastResult = null!;

        for (int i = 0; i < iterations; i++)
        {
            lastResult = await service.SerializeAsync(data, SerializationFormat.MessagePack, CancellationToken.None);
        }

        sw.Stop();

        Console.WriteLine($"  {name}: {sw.ElapsedMilliseconds}ms ({iterations} iterations), {lastResult.Length:N0} bytes");
    }

    static async Task DemoRealWorldScenarios()
    {
        Console.WriteLine("\n3. Real-World Integration Scenarios");

        var service = new MessagePackSerializationService(MessagePackOptions.CreateHighPerformance());

        // Scenario 1: High-frequency event logging
        await DemoEventLogging(service);

        // Scenario 2: Session state persistence
        await DemoSessionPersistence(service);

        // Scenario 3: Analytics batch processing
        await DemoAnalyticsBatch(service);
    }

    static async Task DemoEventLogging(MessagePackSerializationService service)
    {
        Console.WriteLine("\n  Scenario 1: High-Frequency Event Logging");

        var events = new List<GameEvent>();
        for (int i = 0; i < 1000; i++)
        {
            events.Add(new GameEvent
            {
                EventType = i % 3 == 0 ? "Move" : i % 3 == 1 ? "Attack" : "Interact",
                PlayerId = $"player_{i % 10}",
                Timestamp = DateTime.UtcNow.AddMilliseconds(-i),
                Severity = EventSeverity.Info,
                Data = new Dictionary<string, object>
                {
                    ["position"] = new Vector3(i * 0.1f, 0, i * 0.2f),
                    ["value"] = i * 1.5
                }
            });
        }

        var sw = Stopwatch.StartNew();
        var serializedEvents = new List<byte[]>();

        foreach (var evt in events)
        {
            var bytes = await service.SerializeAsync(evt, SerializationFormat.MessagePack, CancellationToken.None);
            serializedEvents.Add(bytes);
        }

        sw.Stop();

        var totalSize = serializedEvents.Sum(b => b.Length);
        Console.WriteLine($"    ✓ Serialized {events.Count} events in {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"    ✓ Total size: {totalSize:N0} bytes ({totalSize / 1024.0:F1} KB)");
        Console.WriteLine($"    ✓ Average size per event: {totalSize / (double)events.Count:F1} bytes");
        Console.WriteLine($"    ✓ Throughput: {events.Count * 1000.0 / sw.ElapsedMilliseconds:F0} events/sec");
    }

    static async Task DemoSessionPersistence(MessagePackSerializationService service)
    {
        Console.WriteLine("\n  Scenario 2: Session State Persistence");

        var sessionState = new SessionState
        {
            PlayerId = "demo_player_001",
            PlayerStats = new PlayerStats
            {
                Level = 42,
                Experience = 250000,
                Health = 95,
                MaxHealth = 100,
                Attributes = new Dictionary<string, int>
                {
                    ["strength"] = 25,
                    ["dexterity"] = 18,
                    ["intelligence"] = 22,
                    ["constitution"] = 20
                }
            },
            Inventory = CreateLargeInventory(),
            WorldState = new WorldState
            {
                CurrentLevel = 5,
                PlayerPosition = new Vector3(125.5f, 10.2f, 89.7f),
                CompletedQuests = Enumerable.Range(1, 50).Select(i => $"quest_{i:D3}").ToList(),
                Variables = new Dictionary<string, object>
                {
                    ["boss_defeated"] = true,
                    ["reputation"] = 85,
                    ["last_checkpoint"] = DateTime.UtcNow.AddMinutes(-5)
                }
            },
            LastSaved = DateTime.UtcNow
        };

        var sw = Stopwatch.StartNew();
        var saveData = await service.SerializeAsync(sessionState, SerializationFormat.MessagePack, CancellationToken.None);
        sw.Stop();
        var saveTime = sw.ElapsedMilliseconds;

        sw.Restart();
        var loadedState = await service.DeserializeAsync<SessionState>(saveData, SerializationFormat.MessagePack, CancellationToken.None);
        sw.Stop();
        var loadTime = sw.ElapsedMilliseconds;

        Console.WriteLine($"    ✓ Save time: {saveTime}ms, Load time: {loadTime}ms");
        Console.WriteLine($"    ✓ Save file size: {saveData.Length:N0} bytes ({saveData.Length / 1024.0:F1} KB)");
        Console.WriteLine($"    ✓ Inventory items: {loadedState.Inventory.Count}");
        Console.WriteLine($"    ✓ Completed quests: {loadedState.WorldState.CompletedQuests.Count}");
    }
   static async Task DemoAnalyticsBatch(MessagePackSerializationService service)
    {
        Console.WriteLine("\n  Scenario 3: Analytics Batch Processing");

        var analyticsData = CreateLargeAnalyticsData();

        var sw = Stopwatch.StartNew();
        var batchData = await service.SerializeAsync(analyticsData, SerializationFormat.MessagePack, CancellationToken.None);
        sw.Stop();

        Console.WriteLine($"    ✓ Batch serialization: {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"    ✓ Batch size: {batchData.Length:N0} bytes ({batchData.Length / 1024.0:F1} KB)");
        Console.WriteLine($"    ✓ Actions in batch: {analyticsData.Actions.Count:N0}");
        Console.WriteLine($"    ✓ Compression ratio: {analyticsData.Actions.Count * 50.0 / batchData.Length:F2} actions per KB");

        // Simulate network transmission efficiency
        var compressionSavings = EstimateJsonSize(analyticsData) - batchData.Length;
        Console.WriteLine($"    ✓ Estimated space savings vs JSON: {compressionSavings:N0} bytes ({compressionSavings / 1024.0:F1} KB)");
    }

    static AnalyticsData CreateLargeAnalyticsData()
    {
        var random = new Random(42);
        var actions = new List<PlayerAction>();

        for (int i = 0; i < 500; i++)
        {
            actions.Add(new PlayerAction
            {
                ActionType = GetRandomActionType(i),
                Timestamp = DateTime.UtcNow.AddSeconds(-i),
                Position = new Vector3(
                    (float)(random.NextDouble() * 100),
                    (float)(random.NextDouble() * 10),
                    (float)(random.NextDouble() * 100)
                ),
                TargetId = i % 5 == 0 ? $"target_{i}" : null,
                Value = random.NextDouble() * 100
            });
        }

        return new AnalyticsData
        {
            SessionId = Guid.NewGuid().ToString(),
            Actions = actions,
            Stats = new PlayerStats
            {
                Level = 30,
                Experience = 180000,
                Health = 90,
                MaxHealth = 100,
                Attributes = new Dictionary<string, int>
                {
                    ["strength"] = 20,
                    ["dexterity"] = 16,
                    ["intelligence"] = 18,
                    ["constitution"] = 17,
                    ["wisdom"] = 14,
                    ["charisma"] = 15
                }
            },
            SessionDuration = TimeSpan.FromMinutes(30),
            Metrics = new Dictionary<string, double>
            {
                ["damage_dealt"] = 1850.5,
                ["damage_taken"] = 650.2,
                ["distance_traveled"] = 950.8,
                ["items_collected"] = 18,
                ["enemies_defeated"] = 35
            }
        };
    }

    static List<InventoryItem> CreateLargeInventory()
    {
        var items = new List<InventoryItem>();
        var itemTypes = new[] { "weapon", "armor", "potion", "scroll", "gem", "material", "tool", "food" };

        for (int i = 0; i < 100; i++)
        {
            items.Add(new InventoryItem
            {
                ItemId = $"{itemTypes[i % itemTypes.Length]}_{i:D3}",
                Quantity = (i % 10) + 1,
                Properties = new Dictionary<string, object>
                {
                    ["rarity"] = i % 5,
                    ["level"] = (i % 20) + 1,
                    ["durability"] = 100 - (i % 30),
                    ["value"] = (i + 1) * 10
                }
            });
        }

        return items;
    }

    static string GetRandomActionType(int index)
    {
        var types = new[] { "move", "attack", "defend", "cast_spell", "use_item", "interact", "jump", "crouch" };
        return types[index % types.Length];
    }

    static int EstimateJsonSize(AnalyticsData data)
    {
        // Rough estimation of JSON size (actual would be larger due to formatting)
        var baseSize = 200; // Base object overhead
        var actionSize = data.Actions.Count * 150; // Estimated 150 bytes per action in JSON
        var statsSize = 100; // Player stats
        var metricsSize = data.Metrics.Count * 30; // Metrics

        return baseSize + actionSize + statsSize + metricsSize;
    }
}
