using FluentAssertions;
using LablabBean.Contracts.AI.Memory;
using LablabBean.Contracts.AI.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.KernelMemory;
using Xunit;

namespace LablabBean.Contracts.AI.Tests.Integration;

/// <summary>
/// Integration tests for memory persistence across application restarts
/// These tests verify that memories stored in Qdrant persist after application shutdown
/// </summary>
[Collection("Integration")]
public class MemoryPersistenceTests : IAsyncLifetime
{
    private ServiceProvider? _serviceProvider;
    private IMemoryService? _memoryService;
    private readonly string _testEntityId = "test-npc-001";
    private readonly string _testMemoryId = "test-memory-restart-001";

    public async Task InitializeAsync()
    {
        // This simulates first application startup
        _serviceProvider = BuildServiceProvider();
        _memoryService = _serviceProvider.GetRequiredService<IMemoryService>();

        // Store a test memory
        var memory = new MemoryEntry
        {
            Id = _testMemoryId,
            EntityId = _testEntityId,
            Content = "Player helped merchant find lost treasure map",
            MemoryType = "quest_interaction",
            Importance = 0.9,
            Timestamp = DateTimeOffset.UtcNow,
            Tags = new Dictionary<string, string>
            {
                { "quest_id", "lost_treasure" },
                { "npc_name", "merchant_john" },
                { "outcome", "success" }
            }
        };

        await _memoryService.StoreMemoryAsync(memory);

        // Give Qdrant time to persist
        await Task.Delay(500);
    }

    public async Task DisposeAsync()
    {
        // Cleanup test data
        if (_memoryService != null)
        {
            try
            {
                await _memoryService.DeleteMemoryAsync(_testMemoryId);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        _serviceProvider?.Dispose();
    }

    [Fact]
    public async Task Memory_ShouldPersist_AfterApplicationRestart()
    {
        // Arrange: Simulate application shutdown and restart
        await DisposeServiceProvider();

        // Wait for graceful shutdown
        await Task.Delay(100);

        // Restart with new service provider (simulates app restart)
        _serviceProvider = BuildServiceProvider();
        _memoryService = _serviceProvider.GetRequiredService<IMemoryService>();

        // Act: Retrieve the memory that was stored before "restart"
        var retrievalOptions = new MemoryRetrievalOptions
        {
            EntityId = _testEntityId,
            Limit = 10,
            MinRelevanceScore = 0.5
        };

        var results = await _memoryService.RetrieveRelevantMemoriesAsync(
            "merchant treasure map quest",
            retrievalOptions);

        // Assert: Memory should still exist
        results.Should().NotBeNull();
        results.Should().HaveCountGreaterThan(0);

        var memory = results.FirstOrDefault(r => r.Memory.Id == _testMemoryId);
        memory.Should().NotBeNull();
        memory!.Memory.Content.Should().Contain("treasure map");
        memory.Memory.EntityId.Should().Be(_testEntityId);
        memory.Memory.Tags.Should().ContainKey("quest_id");
    }

    [Fact]
    public async Task MultipleMemories_ShouldAllPersist_AfterRestart()
    {
        // Arrange: Store multiple memories
        var memories = new[]
        {
            new MemoryEntry
            {
                Id = "mem-restart-001",
                EntityId = _testEntityId,
                Content = "Player traded 50 gold for iron sword",
                MemoryType = "trade",
                Importance = 0.6,
                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-10)
            },
            new MemoryEntry
            {
                Id = "mem-restart-002",
                EntityId = _testEntityId,
                Content = "Player asked about dragon rumors",
                MemoryType = "dialogue",
                Importance = 0.8,
                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-5)
            },
            new MemoryEntry
            {
                Id = "mem-restart-003",
                EntityId = _testEntityId,
                Content = "Player defended merchant from bandits",
                MemoryType = "combat",
                Importance = 0.95,
                Timestamp = DateTimeOffset.UtcNow
            }
        };

        foreach (var mem in memories)
        {
            await _memoryService!.StoreMemoryAsync(mem);
        }

        await Task.Delay(500);

        // Act: Simulate restart
        await DisposeServiceProvider();
        await Task.Delay(100);

        _serviceProvider = BuildServiceProvider();
        _memoryService = _serviceProvider.GetRequiredService<IMemoryService>();

        var retrievalOptions = new MemoryRetrievalOptions
        {
            EntityId = _testEntityId,
            Limit = 10,
            MinRelevanceScore = 0.5
        };

        var results = await _memoryService.RetrieveRelevantMemoriesAsync(
            "player interactions with merchant",
            retrievalOptions);

        // Assert: All memories should be retrievable
        results.Should().HaveCountGreaterThanOrEqualTo(3);

        // Cleanup
        foreach (var mem in memories)
        {
            try
            {
                await _memoryService.DeleteMemoryAsync(mem.Id);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public async Task HighImportanceMemories_ShouldPersist_WithCorrectScores()
    {
        // Arrange: Store memory with high importance
        var criticalMemory = new MemoryEntry
        {
            Id = "mem-critical-001",
            EntityId = _testEntityId,
            Content = "Player saved merchant's life from assassin attack",
            MemoryType = "life_debt",
            Importance = 1.0,
            Timestamp = DateTimeOffset.UtcNow,
            Tags = new Dictionary<string, string>
            {
                { "emotional_impact", "extreme" },
                { "relationship_change", "+50" }
            }
        };

        await _memoryService!.StoreMemoryAsync(criticalMemory);
        await Task.Delay(500);

        // Act: Restart and retrieve
        await DisposeServiceProvider();
        await Task.Delay(100);

        _serviceProvider = BuildServiceProvider();
        _memoryService = _serviceProvider.GetRequiredService<IMemoryService>();

        var retrievalOptions = new MemoryRetrievalOptions
        {
            EntityId = _testEntityId,
            MinImportance = 0.9,
            Limit = 5,
            MinRelevanceScore = 0.5
        };

        var results = await _memoryService.RetrieveRelevantMemoriesAsync(
            "saved life assassin",
            retrievalOptions);

        // Assert
        results.Should().HaveCountGreaterThan(0);
        var memory = results.FirstOrDefault(r => r.Memory.Id == "mem-critical-001");
        memory.Should().NotBeNull();
        memory!.Memory.Importance.Should().Be(1.0);

        // Cleanup
        try
        {
            await _memoryService.DeleteMemoryAsync(criticalMemory.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    private ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "KernelMemory:Storage:Provider", "Qdrant" },
                { "KernelMemory:Storage:ConnectionString", "http://localhost:6333" },
                { "KernelMemory:Storage:CollectionName", "test_memories" }
            })
            .Build();

        services.AddLogging();
        services.AddKernelMemoryService(configuration);

        return services.BuildServiceProvider();
    }

    private async Task DisposeServiceProvider()
    {
        if (_serviceProvider != null)
        {
            await _serviceProvider.DisposeAsync();
            _serviceProvider = null;
            _memoryService = null;
        }
    }
}
