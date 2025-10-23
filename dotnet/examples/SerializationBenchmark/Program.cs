using System.Diagnostics;
using System.Text.Json;
using LablabBean.Contracts.Serialization.Services;
using LablabBean.Contracts.Serialization.Examples;
using LablabBean.Contracts.Serialization.Configuration;
using LablabBean.Contracts.Serialization;

namespace SerializationBenchmark;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("MessagePack vs JSON Performance Benchmark");
        Console.WriteLine("=========================================");

        var messagePackService = new MessagePackSerializationService();
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        // Create test data
        var gameEvent = CreateSampleGameEvent();
        var analyticsData = CreateSampleAnalyticsData();

        Console.WriteLine("\n1. Single Object Serialization Benchmark");
        await BenchmarkSingleObject(messagePackService, jsonOptions, gameEvent);

        Console.WriteLine("\n2. Large Object Serialization Benchmark");
        await BenchmarkLargeObject(messagePackService, jsonOptions, analyticsData);

        Console.WriteLine("\n3. Payload Size Comparison");
        await ComparePayloadSizes(messagePackService, jsonOptions, gameEvent, analyticsData);

        Console.WriteLine("\n4. High-Frequency Operations Benchmark");
        await BenchmarkHighFrequency(messagePackService, jsonOptions, gameEvent);

        Console.WriteLine("\nBenchmark completed!");
    }

    static async Task BenchmarkSingleObject(MessagePackSerializationService msgPackService, JsonSerializerOptions jsonOptions, GameEvent gameEvent)
    {
        const int iterations = 50000;

        // MessagePack benchmark
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            var bytes = await msgPackService.SerializeAsync(gameEvent, SerializationFormat.MessagePack, CancellationToken.None);
            await msgPackService.DeserializeAsync<GameEvent>(bytes, SerializationFormat.MessagePack, CancellationToken.None);
        }
        sw.Stop();
        var msgPackTime = sw.ElapsedMilliseconds;

        // JSON benchmark
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            var json = JsonSerializer.Serialize(gameEvent, jsonOptions);
            JsonSerializer.Deserialize<GameEvent>(json, jsonOptions);
        }
        sw.Stop();
        var jsonTime = sw.ElapsedMilliseconds;

        Console.WriteLine($"MessagePack: {msgPackTime}ms ({iterations:N0} iterations)");
        Console.WriteLine($"JSON: {jsonTime}ms ({iterations:N0} iterations)");
        Console.WriteLine($"MessagePack is {(double)jsonTime / msgPackTime:F2}x faster");
        Console.WriteLine($"Throughput - MessagePack: {iterations * 1000.0 / msgPackTime:F0} ops/sec");
        Console.WriteLine($"Throughput - JSON: {iterations * 1000.0 / jsonTime:F0} ops/sec");
    }    static
 async Task BenchmarkLargeObject(MessagePackSerializationService msgPackService, JsonSerializerOptions jsonOptions, AnalyticsData analyticsData)
    {
        const int iterations = 1000;

        // MessagePack benchmark
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            var bytes = await msgPackService.SerializeAsync(analyticsData, SerializationFormat.MessagePack, CancellationToken.None);
            await msgPackService.DeserializeAsync<AnalyticsData>(bytes, SerializationFormat.MessagePack, CancellationToken.None);
        }
        sw.Stop();
        var msgPackTime = sw.ElapsedMilliseconds;

        // JSON benchmark
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            var json = JsonSerializer.Serialize(analyticsData, jsonOptions);
            JsonSerializer.Deserialize<AnalyticsData>(json, jsonOptions);
        }
        sw.Stop();
        var jsonTime = sw.ElapsedMilliseconds;

        Console.WriteLine($"MessagePack: {msgPackTime}ms ({iterations:N0} iterations, {analyticsData.Actions.Count} actions each)");
        Console.WriteLine($"JSON: {jsonTime}ms ({iterations:N0} iterations, {analyticsData.Actions.Count} actions each)");
        Console.WriteLine($"MessagePack is {(double)jsonTime / msgPackTime:F2}x faster for large objects");
    }

    static async Task ComparePayloadSizes(MessagePackSerializationService msgPackService, JsonSerializerOptions jsonOptions, GameEvent gameEvent, AnalyticsData analyticsData)
    {
        // Game event size comparison
        var msgPackBytes = await msgPackService.SerializeAsync(gameEvent, SerializationFormat.MessagePack, CancellationToken.None);
        var jsonString = JsonSerializer.Serialize(gameEvent, jsonOptions);
        var jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonString);

        Console.WriteLine($"Game Event:");
        Console.WriteLine($"  MessagePack: {msgPackBytes.Length:N0} bytes");
        Console.WriteLine($"  JSON: {jsonBytes.Length:N0} bytes");
        Console.WriteLine($"  MessagePack is {(double)jsonBytes.Length / msgPackBytes.Length:F2}x smaller");

        // Analytics data size comparison
        var msgPackAnalyticsBytes = await msgPackService.SerializeAsync(analyticsData, SerializationFormat.MessagePack, CancellationToken.None);
        var jsonAnalyticsString = JsonSerializer.Serialize(analyticsData, jsonOptions);
        var jsonAnalyticsBytes = System.Text.Encoding.UTF8.GetBytes(jsonAnalyticsString);

        Console.WriteLine($"Analytics Data ({analyticsData.Actions.Count} actions):");
        Console.WriteLine($"  MessagePack: {msgPackAnalyticsBytes.Length:N0} bytes ({msgPackAnalyticsBytes.Length / 1024.0:F1} KB)");
        Console.WriteLine($"  JSON: {jsonAnalyticsBytes.Length:N0} bytes ({jsonAnalyticsBytes.Length / 1024.0:F1} KB)");
        Console.WriteLine($"  MessagePack is {(double)jsonAnalyticsBytes.Length / msgPackAnalyticsBytes.Length:F2}x smaller");
        Console.WriteLine($"  Space saved: {jsonAnalyticsBytes.Length - msgPackAnalyticsBytes.Length:N0} bytes ({(jsonAnalyticsBytes.Length - msgPackAnalyticsBytes.Length) / 1024.0:F1} KB)");
    }

    static async Task BenchmarkHighFrequency(MessagePackSerializationService msgPackService, JsonSerializerOptions jsonOptions, GameEvent gameEvent)
    {
        const int iterations = 100000;
        const int warmupIterations = 1000;

        // Warmup
        for (int i = 0; i < warmupIterations; i++)
        {
            await msgPackService.SerializeAsync(gameEvent, SerializationFormat.MessagePack, CancellationToken.None);
            JsonSerializer.Serialize(gameEvent, jsonOptions);
        }

        // High-frequency MessagePack test
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            await msgPackService.SerializeAsync(gameEvent, SerializationFormat.MessagePack, CancellationToken.None);
        }
        sw.Stop();
        var msgPackTime = sw.ElapsedMilliseconds;

        // High-frequency JSON test
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            JsonSerializer.Serialize(gameEvent, jsonOptions);
        }
        sw.Stop();
        var jsonTime = sw.ElapsedMilliseconds;

        Console.WriteLine($"High-frequency serialization ({iterations:N0} operations):");
        Console.WriteLine($"MessagePack: {msgPackTime}ms ({iterations * 1000.0 / msgPackTime:F0} ops/sec)");
        Console.WriteLine($"JSON: {jsonTime}ms ({iterations * 1000.0 / jsonTime:F0} ops/sec)");
        Console.WriteLine($"MessagePack is {(double)jsonTime / msgPackTime:F2}x faster");

        if (msgPackTime > 0)
        {
            var avgMsgPackLatency = (double)msgPackTime / iterations;
            Console.WriteLine($"Average MessagePack latency: {avgMsgPackLatency:F4}ms per operation");
        }

        if (jsonTime > 0)
        {
            var avgJsonLatency = (double)jsonTime / iterations;
            Console.WriteLine($"Average JSON latency: {avgJsonLatency:F4}ms per operation");
        }
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
                ["duration"] = 1.2,
                ["weapon"] = "sword_001",
                ["health"] = 85,
                ["mana"] = 120,
                ["level"] = 25
            }
        };
    }

    static AnalyticsData CreateSampleAnalyticsData()
    {
        var actions = new List<PlayerAction>();
        var random = new Random(42); // Fixed seed for consistent results

        for (int i = 0; i < 2000; i++) // Increased for more realistic test
        {
            actions.Add(new PlayerAction
            {
                ActionType = i % 3 == 0 ? "move" : i % 3 == 1 ? "attack" : "interact",
                Timestamp = DateTime.UtcNow.AddSeconds(-i),
                Position = new Vector3(
                    (float)(random.NextDouble() * 100),
                    (float)(random.NextDouble() * 10),
                    (float)(random.NextDouble() * 100)
                ),
                TargetId = i % 7 == 0 ? $"enemy_{i}" : null,
                Value = random.NextDouble() * 100
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
                    ["constitution"] = 15,
                    ["wisdom"] = 12,
                    ["charisma"] = 13
                }
            },
            SessionDuration = TimeSpan.FromMinutes(45),
            Metrics = new Dictionary<string, double>
            {
                ["damage_dealt"] = 2450.5,
                ["damage_taken"] = 890.2,
                ["distance_traveled"] = 1250.8,
                ["items_collected"] = 23,
                ["enemies_defeated"] = 45,
                ["quests_completed"] = 3,
                ["gold_earned"] = 1250,
                ["experience_gained"] = 5500
            }
        };
    }
}
