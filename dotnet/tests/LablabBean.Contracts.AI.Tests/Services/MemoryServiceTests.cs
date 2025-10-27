using FluentAssertions;
using LablabBean.Contracts.AI.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LablabBean.Contracts.AI.Tests.Services;

public class MemoryServiceTests
{
    private readonly IMemoryService _sut;

    public MemoryServiceTests()
    {
        _sut = new TestMemoryService(NullLogger<TestMemoryService>.Instance);
    }

    [Fact]
    public async Task StoreMemoryAsync_WithValidEntry_ReturnsMemoryId()
    {
        // Arrange
        var memoryEntry = new MemoryEntry
        {
            Id = "mem-123",
            Content = "Player was friendly to merchant",
            EntityId = "player-123",
            MemoryType = "npc_interaction",
            Importance = 0.8,
            Tags = new Dictionary<string, string>
            {
                { "npc_id", "merchant-001" },
                { "location", "village_square" }
            }
        };

        // Act
        var memoryId = await _sut.StoreMemoryAsync(memoryEntry);

        // Assert
        memoryId.Should().NotBeNullOrEmpty();
        memoryId.Should().Be("mem-123");
    }

    [Fact]
    public async Task StoreMemoryAsync_WithNullEntry_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _sut.StoreMemoryAsync(null!));
    }

    [Fact]
    public async Task RetrieveRelevantMemoriesAsync_WithValidQuery_ReturnsRelevantMemories()
    {
        // Arrange
        var memoryEntry = new MemoryEntry
        {
            Id = "mem-456",
            Content = "Player helped the merchant find lost goods",
            EntityId = "player-123",
            MemoryType = "npc_interaction",
            Importance = 0.9,
            Tags = new Dictionary<string, string> { { "npc_id", "merchant-001" } }
        };
        await _sut.StoreMemoryAsync(memoryEntry);

        var options = new MemoryRetrievalOptions
        {
            EntityId = "player-123",
            Limit = 5,
            MinRelevanceScore = 0.5
        };

        // Act
        var results = await _sut.RetrieveRelevantMemoriesAsync("merchant interaction", options);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCountGreaterThan(0);
        results.First().Memory.Content.Should().Contain("merchant");
        results.First().RelevanceScore.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RetrieveRelevantMemoriesAsync_WithEmptyQuery_ThrowsArgumentException()
    {
        // Arrange
        var options = new MemoryRetrievalOptions
        {
            EntityId = "player-123",
            Limit = 5
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _sut.RetrieveRelevantMemoriesAsync("", options));
    }

    [Fact]
    public async Task RetrieveRelevantMemoriesAsync_WithMinImportanceFilter_ReturnsOnlyImportantMemories()
    {
        // Arrange
        await _sut.StoreMemoryAsync(new MemoryEntry
        {
            Id = "mem-low",
            Content = "Player walked past the merchant",
            EntityId = "player-123",
            MemoryType = "observation",
            Importance = 0.3
        });

        await _sut.StoreMemoryAsync(new MemoryEntry
        {
            Id = "mem-high",
            Content = "Player saved the merchant from bandits",
            EntityId = "player-123",
            MemoryType = "heroic_action",
            Importance = 0.95
        });

        var options = new MemoryRetrievalOptions
        {
            EntityId = "player-123",
            MinImportance = 0.8,
            Limit = 10
        };

        // Act
        var results = await _sut.RetrieveRelevantMemoriesAsync("merchant", options);

        // Assert
        results.Should().HaveCount(1);
        results.First().Memory.Importance.Should().BeGreaterThanOrEqualTo(0.8);
    }

    [Fact]
    public async Task GetMemoryByIdAsync_WithExistingId_ReturnsMemory()
    {
        // Arrange
        var memoryEntry = new MemoryEntry
        {
            Id = "mem-789",
            Content = "Test memory content",
            EntityId = "player-123",
            MemoryType = "test",
            Importance = 0.5
        };
        await _sut.StoreMemoryAsync(memoryEntry);

        // Act
        var result = await _sut.GetMemoryByIdAsync("mem-789");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("mem-789");
        result.Content.Should().Be("Test memory content");
    }

    [Fact]
    public async Task GetMemoryByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Act
        var result = await _sut.GetMemoryByIdAsync("non-existent-id");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateMemoryImportanceAsync_WithValidData_UpdatesSuccessfully()
    {
        // Arrange
        var memoryEntry = new MemoryEntry
        {
            Id = "mem-update",
            Content = "Initial memory",
            EntityId = "player-123",
            MemoryType = "test",
            Importance = 0.5
        };
        await _sut.StoreMemoryAsync(memoryEntry);

        // Act
        await _sut.UpdateMemoryImportanceAsync("mem-update", 0.9);

        // Assert
        var updated = await _sut.GetMemoryByIdAsync("mem-update");
        updated.Should().NotBeNull();
        updated!.Importance.Should().Be(0.9);
    }

    [Fact]
    public async Task UpdateMemoryImportanceAsync_WithInvalidImportance_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await _sut.UpdateMemoryImportanceAsync("mem-123", 1.5));
    }

    [Fact]
    public async Task CleanOldMemoriesAsync_RemovesOldMemories()
    {
        // Arrange
        var entityId = "player-clean-test";
        var oldMemory = new MemoryEntry
        {
            Id = "mem-old",
            Content = "Old memory",
            EntityId = entityId,
            MemoryType = "test",
            Importance = 0.5,
            Timestamp = DateTimeOffset.UtcNow.AddDays(-10)
        };

        var recentMemory = new MemoryEntry
        {
            Id = "mem-recent",
            Content = "Recent memory",
            EntityId = entityId,
            MemoryType = "test",
            Importance = 0.5,
            Timestamp = DateTimeOffset.UtcNow
        };

        await _sut.StoreMemoryAsync(oldMemory);
        await _sut.StoreMemoryAsync(recentMemory);

        // Act
        var deletedCount = await _sut.CleanOldMemoriesAsync(entityId, TimeSpan.FromDays(7));

        // Assert
        deletedCount.Should().Be(1);

        var remaining = await _sut.GetMemoryByIdAsync("mem-recent");
        remaining.Should().NotBeNull();

        var removed = await _sut.GetMemoryByIdAsync("mem-old");
        removed.Should().BeNull();
    }

    [Fact]
    public async Task IsHealthyAsync_ReturnsTrue()
    {
        // Act
        var isHealthy = await _sut.IsHealthyAsync();

        // Assert
        isHealthy.Should().BeTrue();
    }

    private class TestMemoryService : IMemoryService
    {
        private readonly Dictionary<string, MemoryEntry> _storage = new();
        private readonly ILogger _logger;

        public TestMemoryService(ILogger logger)
        {
            _logger = logger;
        }

        public Task<string> StoreMemoryAsync(MemoryEntry memory, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(memory);
            _storage[memory.Id] = memory;
            return Task.FromResult(memory.Id);
        }

        public Task<IReadOnlyList<MemoryResult>> RetrieveRelevantMemoriesAsync(
            string queryText,
            MemoryRetrievalOptions options,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(queryText))
                throw new ArgumentException("Query text cannot be empty", nameof(queryText));

            var queryWords = queryText.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var results = _storage.Values
                .Where(m => string.IsNullOrEmpty(options.EntityId) || m.EntityId == options.EntityId)
                .Where(m => string.IsNullOrEmpty(options.MemoryType) || m.MemoryType == options.MemoryType)
                .Where(m => m.Importance >= options.MinImportance)
                .Where(m => !options.FromTimestamp.HasValue || m.Timestamp >= options.FromTimestamp.Value)
                .Where(m => !options.ToTimestamp.HasValue || m.Timestamp <= options.ToTimestamp.Value)
                .Select(m =>
                {
                    var contentWords = m.Content.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var matchCount = queryWords.Count(qw => contentWords.Contains(qw));
                    var relevance = queryWords.Length > 0 ? (double)matchCount / queryWords.Length : 0;

                    return new MemoryResult
                    {
                        Memory = m,
                        RelevanceScore = relevance,
                        Source = "test"
                    };
                })
                .Where(r => r.RelevanceScore >= options.MinRelevanceScore)
                .OrderByDescending(r => r.RelevanceScore)
                .Take(options.Limit)
                .ToList();

            return Task.FromResult<IReadOnlyList<MemoryResult>>(results);
        }

        public Task<MemoryEntry?> GetMemoryByIdAsync(string memoryId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(memoryId))
                throw new ArgumentException("Memory ID cannot be empty", nameof(memoryId));

            _storage.TryGetValue(memoryId, out var memory);
            return Task.FromResult(memory);
        }

        public async Task UpdateMemoryImportanceAsync(
            string memoryId,
            double importance,
            CancellationToken cancellationToken = default)
        {
            if (importance < 0.0 || importance > 1.0)
                throw new ArgumentOutOfRangeException(nameof(importance));

            var memory = await GetMemoryByIdAsync(memoryId, cancellationToken);
            if (memory == null)
                throw new InvalidOperationException($"Memory {memoryId} not found");

            var updated = memory with { Importance = importance };
            _storage[memoryId] = updated;
        }

        public Task DeleteMemoryAsync(string memoryId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(memoryId))
                throw new ArgumentException("Memory ID cannot be empty", nameof(memoryId));

            _storage.Remove(memoryId);
            return Task.CompletedTask;
        }

        public async Task<int> CleanOldMemoriesAsync(
            string entityId,
            TimeSpan olderThan,
            CancellationToken cancellationToken = default)
        {
            var cutoffTime = DateTimeOffset.UtcNow - olderThan;

            var oldMemories = _storage.Values
                .Where(m => m.EntityId == entityId && m.Timestamp < cutoffTime)
                .ToList();

            foreach (var memory in oldMemories)
            {
                _storage.Remove(memory.Id);
            }

            return await Task.FromResult(oldMemories.Count);
        }

        public Task<bool> IsHealthyAsync()
        {
            return Task.FromResult(true);
        }

        public Task<string> StoreTacticalObservationAsync(
            string entityId,
            TacticalObservation observation,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(observation);
            var memoryId = Guid.NewGuid().ToString();
            return Task.FromResult(memoryId);
        }

        public Task<IReadOnlyList<MemoryResult>> RetrieveSimilarTacticsAsync(
            string entityId,
            LablabBean.AI.Core.Events.PlayerBehaviorType behaviorFilter,
            int limit = 5,
            CancellationToken cancellationToken = default)
        {
            var results = new List<MemoryResult>();
            return Task.FromResult<IReadOnlyList<MemoryResult>>(results);
        }

        public Task<string> StoreRelationshipMemoryAsync(
            RelationshipMemory relationshipMemory,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(relationshipMemory);
            var memoryId = Guid.NewGuid().ToString();
            return Task.FromResult(memoryId);
        }

        public Task<IReadOnlyList<MemoryResult>> RetrieveRelevantRelationshipHistoryAsync(
            string entity1Id,
            string entity2Id,
            string query,
            int maxResults = 5,
            string? sentiment = null,
            CancellationToken cancellationToken = default)
        {
            var results = new List<MemoryResult>();
            return Task.FromResult<IReadOnlyList<MemoryResult>>(results);
        }
    }
}
