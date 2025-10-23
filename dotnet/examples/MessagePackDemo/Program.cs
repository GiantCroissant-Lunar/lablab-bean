using System.Diagnostics;
using LablabBean.Contracts.Serialization.Services;
using LablabBean.Contracts.Serialization.Examples;
using LablabBean.Contracts.Serialization.Configuration;
using LablabBean.Contracts.Serialization;

namespace MessagePackDemo;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("MessagePack Serialization Demo");
        Console.WriteLine("==============================");

        // Create services with different configurations
        var defaultService = new MessagePackSerializationService();
        var highPerfService = new MessagePackSerializationService(MessagePackOptions.CreateHighPerformance());
        var compactService = new MessagePackSerializationService(MessagePackOptions.CreateCompact());

        // Create test data
        var gameEvent = CreateSampleGameEvent();
        var analyticsData = CreateSampleAnalyticsData();
        var sessionState = CreateSampleSessionState();

        Console.WriteLine("\n1. Basic Serialization Test");
        await TestBasicSerialization(defaultService, gameEvent);

        Console.WriteLine("\n2. Performance Comparison");
        await TestPerformanceComparison(defaultService, highPerfService, compactService, gameEvent);

        Console.WriteLine("\n3. Large Object Test");
        await TestLargeObjectSerialization(defaultService, analyticsData);

        Console.WriteLine("\n4. Session State Persistence");
        await TestSessionStatePersistence(defaultService, sessionState);

        Console.WriteLine("\n5. Stream Serialization Test");
        await TestStreamSerialization(defaultService, gameEvent);

        Console.WriteLine("\nDemo completed successfully!");
    }

    static GameEvent CreateSampleGameEvent()
    {
        return new GameEvent
        {
            EventType = "PlayerAction",
            Timestamp = DateTime.UtcNow,
            PlayerId = "player_12345",
            Severity = EventSeverity.Info,
            Data = new Dictionary<string, object>
            {
                ["action"] = "move",
                ["from"] = new Vector3(10.5f, 0, 15.2f),
                ["to"] = new Vector3(12.1f, 0, 16.8f),
                ["speed"] = 5.5,
                ["duration"] = 1.2
            }
        };
    }    static
 AnalyticsData CreateSampleAnalyticsData()
    {
        var actions = new List<PlayerAction>();
        for (int i = 0; i < 1000; i++)
        {
            actions.Add(new PlayerAction
            {
                ActionType = i % 2 == 0 ? "move" : "attack",
                Timestamp = DateTime.UtcNow.AddSeconds(-i),
                Position = new Vector3(i * 0.1f, 0, i * 0.2f),
                TargetId = i % 5 == 0 ? $"enemy_{i}" : null,
                Value = i * 1.5
            });
        }

        return new AnalyticsData
        {
            SessionId = Guid.NewGuid().ToString(),
            Actions = actions,
            Stats = new PlayerStats
            {
                Level = 25,
                Experience = 125000,
                Health = 85,
                MaxHealth = 100,
                Attributes = new Dictionary<string, int>
                {
                    ["strength"] = 18,
                    ["dexterity"] = 14,
                    ["intelligence"] = 16,
                    ["constitution"] = 15
                }
            },
            SessionDuration = TimeSpan.FromMinutes(45),
            Metrics = new Dictionary<string, double>
            {
                ["damage_dealt"] = 2450.5,
                ["damage_taken"] = 890.2,
                ["distance_traveled"] = 1250.8,
                ["items_collected"] = 23,
                ["enemies_defeated"] = 45
            }
        };
    }

    static SessionState CreateSampleSessionState()
    {
        return new SessionState
        {
            PlayerId = "player_12345",
            PlayerStats = new PlayerStats
            {
                Level = 25,
                Experience = 125000,
                Health = 85,
                MaxHealth = 100,
                Attributes = new Dictionary<string, int>
                {
                    ["strength"] = 18,
                    ["dexterity"] = 14,
                    ["intelligence"] = 16
                }
            },
            Inventory = new List<InventoryItem>
            {
                new() { ItemId = "sword_001", Quantity = 1, Properties = new() { ["damage"] = 25, ["durability"] = 100 } },
                new() { ItemId = "potion_health", Quantity = 5, Properties = new() { ["healing"] = 50 } },
                new() { ItemId = "gold", Quantity = 1250, Properties = new() }
            },
            WorldState = new WorldState
            {
                CurrentLevel = 3,
                PlayerPosition = new Vector3(125.5f, 10.2f, 89.7f),
                CompletedQuests = new List<string> { "tutorial", "first_dungeon", "rescue_villager" },
                Variables = new Dictionary<string, object>
                {
                    ["has_key"] = true,
                    ["boss_defeated"] = false,
                    ["reputation"] = 75
                }
            },
            Achievements = new Dictionary<string, bool>
            {
                ["first_kill"] = true,
                ["level_10"] = true,
                ["treasure_hunter"] = false
            },
            LastSaved = DateTime.UtcNow
        };
    }
   static async Task TestBasicSerialization(MessagePackSerializationService service, GameEvent gameEvent)
    {
        try
        {
            // Test byte array serialization
            var bytes = await service.SerializeAsync(gameEvent, SerializationFormat.MessagePack, CancellationToken.None);
            var deserialized = await service.DeserializeAsync<GameEvent>(bytes, SerializationFormat.MessagePack, CancellationToken.None);

            Console.WriteLine($"✓ Serialized to {bytes.Length} bytes");
            Console.WriteLine($"✓ Event type: {deserialized.EventType}");
            Console.WriteLine($"✓ Player ID: {deserialized.PlayerId}");
            Console.WriteLine($"✓ Data items: {deserialized.Data.Count}");

            // Test string serialization (Base64)
            var base64 = await service.SerializeToStringAsync(gameEvent, SerializationFormat.MessagePack, CancellationToken.None);
            var fromBase64 = await service.DeserializeFromStringAsync<GameEvent>(base64, SerializationFormat.MessagePack, CancellationToken.None);

            Console.WriteLine($"✓ Base64 length: {base64.Length}");
            Console.WriteLine($"✓ Round-trip successful: {fromBase64.EventType == gameEvent.EventType}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }

    static async Task TestPerformanceComparison(
        MessagePackSerializationService defaultService,
        MessagePackSerializationService highPerfService,
        MessagePackSerializationService compactService,
        GameEvent gameEvent)
    {
        const int iterations = 10000;

        // Test default configuration
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            var bytes = await defaultService.SerializeAsync(gameEvent, SerializationFormat.MessagePack, CancellationToken.None);
            await defaultService.DeserializeAsync<GameEvent>(bytes, SerializationFormat.MessagePack, CancellationToken.None);
        }
        sw.Stop();
        var defaultTime = sw.ElapsedMilliseconds;

        // Test high performance configuration
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            var bytes = await highPerfService.SerializeAsync(gameEvent, SerializationFormat.MessagePack, CancellationToken.None);
            await highPerfService.DeserializeAsync<GameEvent>(bytes, SerializationFormat.MessagePack, CancellationToken.None);
        }
        sw.Stop();
        var highPerfTime = sw.ElapsedMilliseconds;

        // Test compact configuration
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            var bytes = await compactService.SerializeAsync(gameEvent, SerializationFormat.MessagePack, CancellationToken.None);
            await compactService.DeserializeAsync<GameEvent>(bytes, SerializationFormat.MessagePack, CancellationToken.None);
        }
        sw.Stop();
        var compactTime = sw.ElapsedMilliseconds;

        Console.WriteLine($"Default config: {defaultTime}ms ({iterations} iterations)");
        Console.WriteLine($"High perf config: {highPerfTime}ms ({iterations} iterations)");
        Console.WriteLine($"Compact config: {compactTime}ms ({iterations} iterations)");
        Console.WriteLine($"High perf improvement: {(double)defaultTime / highPerfTime:F2}x faster");
    }
 static async Task TestLargeObjectSerialization(MessagePackSerializationService service, AnalyticsData analyticsData)
    {
        try
        {
            var sw = Stopwatch.StartNew();
            var bytes = await service.SerializeAsync(analyticsData, SerializationFormat.MessagePack, CancellationToken.None);
            sw.Stop();

            Console.WriteLine($"✓ Serialized {analyticsData.Actions.Count} actions in {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"✓ Payload size: {bytes.Length:N0} bytes ({bytes.Length / 1024.0:F1} KB)");

            sw.Restart();
            var deserialized = await service.DeserializeAsync<AnalyticsData>(bytes, SerializationFormat.MessagePack, CancellationToken.None);
            sw.Stop();

            Console.WriteLine($"✓ Deserialized in {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"✓ Actions count: {deserialized.Actions.Count}");
            Console.WriteLine($"✓ Session duration: {deserialized.SessionDuration}");
            Console.WriteLine($"✓ Metrics count: {deserialized.Metrics.Count}");

            // Test size estimation
            var estimatedSize = service.GetEstimatedSize(analyticsData, SerializationFormat.MessagePack);
            Console.WriteLine($"✓ Size estimation: {estimatedSize:N0} bytes (actual: {bytes.Length:N0})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }

    static async Task TestSessionStatePersistence(MessagePackSerializationService service, SessionState sessionState)
    {
        try
        {
            var fileName = "session_state.msgpack";

            // Save to file
            using (var fileStream = File.Create(fileName))
            {
                await service.SerializeToStreamAsync(sessionState, fileStream, SerializationFormat.MessagePack, CancellationToken.None);
            }

            var fileInfo = new FileInfo(fileName);
            Console.WriteLine($"✓ Saved session state to {fileName} ({fileInfo.Length:N0} bytes)");

            // Load from file
            using (var fileStream = File.OpenRead(fileName))
            {
                var loaded = await service.DeserializeFromStreamAsync<SessionState>(fileStream, SerializationFormat.MessagePack, CancellationToken.None);

                Console.WriteLine($"✓ Loaded session for player: {loaded.PlayerId}");
                Console.WriteLine($"✓ Player level: {loaded.PlayerStats.Level}");
                Console.WriteLine($"✓ Inventory items: {loaded.Inventory.Count}");
                Console.WriteLine($"✓ Current level: {loaded.WorldState.CurrentLevel}");
                Console.WriteLine($"✓ Completed quests: {loaded.WorldState.CompletedQuests.Count}");
            }

            // Clean up
            File.Delete(fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }

    static async Task TestStreamSerialization(MessagePackSerializationService service, GameEvent gameEvent)
    {
        try
        {
            using var memoryStream = new MemoryStream();

            // Serialize to stream
            await service.SerializeToStreamAsync(gameEvent, memoryStream, SerializationFormat.MessagePack, CancellationToken.None);

            Console.WriteLine($"✓ Serialized to stream: {memoryStream.Length} bytes");

            // Reset stream position for reading
            memoryStream.Position = 0;

            // Deserialize from stream
            var deserialized = await service.DeserializeFromStreamAsync<GameEvent>(memoryStream, SerializationFormat.MessagePack, CancellationToken.None);

            Console.WriteLine($"✓ Deserialized from stream: {deserialized.EventType}");
            Console.WriteLine($"✓ Stream round-trip successful");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }
}
