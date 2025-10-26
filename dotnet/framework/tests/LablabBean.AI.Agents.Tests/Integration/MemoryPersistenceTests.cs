using FluentAssertions;
using LablabBean.AI.Agents.Configuration;
using LablabBean.Contracts.AI.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.KernelMemory;
using NSubstitute;

namespace LablabBean.AI.Agents.Tests.Integration;

/// <summary>
/// Integration tests for memory persistence across application restarts
/// Tests T030 - Verifies memories persist when using Qdrant and survive service recreation
/// </summary>
public class MemoryPersistenceTests : IAsyncLifetime
{
    private readonly ILogger<LablabBean.AI.Agents.Services.MemoryService> _logger;
    private IOptions<KernelMemoryOptions> _options;
    private IKernelMemory? _kernelMemory;
    private LablabBean.AI.Agents.Services.MemoryService? _memoryService;
    private const string TestCollectionName = "persistence-test-memories";
    private const string TestEntityId = "test-npc-001";

    public MemoryPersistenceTests()
    {
        _logger = Substitute.For<ILogger<LablabBean.AI.Agents.Services.MemoryService>>();

        // Configure with Qdrant for persistence testing
        // NOTE: This test requires Qdrant to be running (docker-compose up -d)
        // If Qdrant is unavailable, the test will be skipped
        var memoryOptions = new KernelMemoryOptions
        {
            Embedding = new EmbeddingOptions
            {
                Provider = "OpenAI",
                ModelName = "text-embedding-3-small",
                MaxTokens = 8191,
                Endpoint = null
            },
            Storage = new StorageOptions
            {
                Provider = "Qdrant", // Persistent storage for testing
                ConnectionString = "http://localhost:6333",
                CollectionName = TestCollectionName
            },
            TextGeneration = new TextGenerationOptions
            {
                Provider = "OpenAI",
                ModelName = "gpt-4",
                MaxTokens = 4096,
                Endpoint = null
            }
        };

        _options = Options.Create(memoryOptions);
    }

    public async Task InitializeAsync()
    {
        // Create mock KernelMemory for tests
        _kernelMemory = Substitute.For<IKernelMemory>();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        // Cleanup: Clear test data
        // TODO: Clean up Qdrant test collection (once Qdrant integration is implemented)
        // TODO: Implement IAsyncDisposable in MemoryService (T040)
        _memoryService = null;
        await Task.CompletedTask;
    }

    #region T030: Memory Persistence Across Restarts

    [Fact(Skip = "Requires Qdrant to be running - will be enabled after T032-T036 are complete")]
    public async Task MemoriesPersistAcrossRestart_StoreAndRecreate_ShouldRetrieveSameMemories()
    {
        // This test verifies the core user story for US2:
        // "NPCs should retain memories across application restarts"

        // Arrange - Create test memories
        var memory1 = new MemoryEntry
        {
            Id = Guid.NewGuid().ToString(),
            Content = "Player used aggressive rushing tactics in combat",
            EntityId = TestEntityId,
            MemoryType = "combat",
            Importance = 0.8,
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-50),
            Tags = new Dictionary<string, string>
            {
                ["type"] = "tactical_observation",
                ["behavior"] = "aggressive"
            }
        };

        var memory2 = new MemoryEntry
        {
            Id = Guid.NewGuid().ToString(),
            Content = "Player unlocked Dragon Slayer achievement",
            EntityId = TestEntityId,
            MemoryType = "achievement",
            Importance = 1.0,
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-5),
            Tags = new Dictionary<string, string>
            {
                ["type"] = "achievement",
                ["name"] = "dragon_slayer"
            }
        };

        // Act Part 1: Store memories with first service instance
        _memoryService = new LablabBean.AI.Agents.Services.MemoryService(_logger, _kernelMemory!, _options);

        await _memoryService.StoreMemoryAsync(memory1);
        await _memoryService.StoreMemoryAsync(memory2);

        // Simulate application restart: Set service to null (simulating disposal)
        _memoryService = null;

        // Act Part 2: Create new service instance (simulating restart)
        var memoryServiceAfterRestart = new LablabBean.AI.Agents.Services.MemoryService(
            _logger,
            _kernelMemory!,
            _options
        );

        // Retrieve memories using semantic search
        var retrievalOptions = new MemoryRetrievalOptions
        {
            EntityId = TestEntityId,
            Limit = 10,
            MinRelevanceScore = 0.5
        };

        var retrievedMemories = await memoryServiceAfterRestart.RetrieveRelevantMemoriesAsync(
            "tactical combat observations and achievements",
            retrievalOptions
        );

        // Assert: Memories should be retrievable after restart
        retrievedMemories.Should().NotBeEmpty("memories should persist across restart");
        retrievedMemories.Should().HaveCountGreaterOrEqualTo(2, "should retrieve both stored memories");
    }

    [Fact(Skip = "Requires Qdrant to be running - will be enabled after T032-T036 are complete")]
    public async Task MemoriesPersistAcrossMultipleRestarts_StoreRecreateMultipleTimes_ShouldMaintainData()
    {
        // This test verifies memories survive multiple restart cycles

        // Arrange
        var memory1 = new MemoryEntry
        {
            Id = Guid.NewGuid().ToString(),
            Content = "First test memory from cycle 1",
            EntityId = TestEntityId,
            MemoryType = "test_event_1",
            Importance = 0.8,
            Timestamp = DateTimeOffset.UtcNow,
            Tags = new Dictionary<string, string> { ["cycle"] = "1" }
        };

        var memory2 = new MemoryEntry
        {
            Id = Guid.NewGuid().ToString(),
            Content = "Second test memory from cycle 2",
            EntityId = TestEntityId,
            MemoryType = "test_event_2",
            Importance = 0.8,
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(1),
            Tags = new Dictionary<string, string> { ["cycle"] = "2" }
        };

        var memory3 = new MemoryEntry
        {
            Id = Guid.NewGuid().ToString(),
            Content = "Third test memory from cycle 3",
            EntityId = TestEntityId,
            MemoryType = "test_event_3",
            Importance = 0.8,
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(2),
            Tags = new Dictionary<string, string> { ["cycle"] = "3" }
        };

        // Act: Cycle 1 - Store first memory
        _memoryService = new LablabBean.AI.Agents.Services.MemoryService(_logger, _kernelMemory!, _options);
        await _memoryService.StoreMemoryAsync(memory1);
        _memoryService = null; // Simulate restart

        // Act: Cycle 2 - Recreate and store second memory
        _memoryService = new LablabBean.AI.Agents.Services.MemoryService(_logger, _kernelMemory!, _options);
        await _memoryService.StoreMemoryAsync(memory2);
        _memoryService = null; // Simulate restart

        // Act: Cycle 3 - Recreate and store third memory
        _memoryService = new LablabBean.AI.Agents.Services.MemoryService(_logger, _kernelMemory!, _options);
        await _memoryService.StoreMemoryAsync(memory3);
        _memoryService = null; // Simulate restart

        // Act: Cycle 4 - Recreate and retrieve all memories
        _memoryService = new LablabBean.AI.Agents.Services.MemoryService(_logger, _kernelMemory!, _options);

        var allMemories = await _memoryService.RetrieveRelevantMemoriesAsync(
            "test memory from cycle",
            new MemoryRetrievalOptions
            {
                EntityId = TestEntityId,
                Limit = 10,
                MinRelevanceScore = 0.5
            }
        );

        // Assert: All 3 memories should be present
        allMemories.Should().HaveCount(3, "all memories from all cycles should persist");

        var cycles = allMemories.Select(m => m.Memory.Tags["cycle"]).ToList();
        cycles.Should().Contain("1", "memory from cycle 1 should exist");
        cycles.Should().Contain("2", "memory from cycle 2 should exist");
        cycles.Should().Contain("3", "memory from cycle 3 should exist");
    }

    [Fact(Skip = "Requires Qdrant to be running - will be enabled after T037 is complete")]
    public async Task QdrantUnavailable_ShouldFallbackToInMemoryGracefully()
    {
        // This test verifies graceful degradation (T037)
        // When Qdrant is unavailable, system should fall back to in-memory storage

        // Arrange: Configure with invalid Qdrant endpoint
        var invalidMemoryOptions = new KernelMemoryOptions
        {
            Embedding = new EmbeddingOptions
            {
                Provider = "OpenAI",
                ModelName = "text-embedding-3-small",
                MaxTokens = 8191,
                Endpoint = null
            },
            Storage = new StorageOptions
            {
                Provider = "Qdrant",
                ConnectionString = "http://localhost:9999", // Invalid port
                CollectionName = "test-collection"
            },
            TextGeneration = new TextGenerationOptions
            {
                Provider = "OpenAI",
                ModelName = "gpt-4",
                MaxTokens = 4096,
                Endpoint = null
            }
        };

        var invalidOptions = Options.Create(invalidMemoryOptions);

        // Act: Create service with invalid Qdrant config
        var memoryService = new LablabBean.AI.Agents.Services.MemoryService(
            _logger,
            _kernelMemory!,
            invalidOptions
        );

        // System should NOT throw, but fall back to in-memory
        var testMemory = new MemoryEntry
        {
            Id = Guid.NewGuid().ToString(),
            Content = "Test memory for fallback scenario",
            EntityId = "fallback-test-npc",
            MemoryType = "test",
            Importance = 0.7,
            Timestamp = DateTimeOffset.UtcNow,
            Tags = new Dictionary<string, string> { ["test"] = "fallback" }
        };

        // Assert: Should not throw exception
        var storeAction = async () => await memoryService.StoreMemoryAsync(testMemory);
        await storeAction.Should().NotThrowAsync("system should gracefully fall back to in-memory");

        // Verify memory was stored (even if in-memory)
        var retrieved = await memoryService.RetrieveRelevantMemoriesAsync(
            "test memory fallback",
            new MemoryRetrievalOptions
            {
                EntityId = "fallback-test-npc",
                Limit = 1,
                MinRelevanceScore = 0.5
            }
        );

        retrieved.Should().ContainSingle("memory should be stored even with fallback");
    }

    #endregion
}
