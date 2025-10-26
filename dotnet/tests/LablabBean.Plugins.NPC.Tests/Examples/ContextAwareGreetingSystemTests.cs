using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LablabBean.Contracts.AI.Memory;
using LablabBean.Plugins.NPC.Examples;
using LablabBean.Plugins.NPC.Services;
using LablabBean.Plugins.NPC.Systems;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LablabBean.Plugins.NPC.Tests.Examples;

public class ContextAwareGreetingSystemTests
{
    private readonly TestMemoryService _memoryService;
    private readonly TestNPCService _npcService;
    private readonly ContextAwareGreetingSystem _sut;

    public ContextAwareGreetingSystemTests()
    {
        _memoryService = new TestMemoryService();
        _npcService = new TestNPCService(_memoryService);
        _sut = new ContextAwareGreetingSystem(
            _npcService,
            NullLogger<ContextAwareGreetingSystem>.Instance
        );
    }

    [Fact]
    public async Task GenerateGreetingAsync_Stranger_ReturnsGenericGreeting()
    {
        // Arrange
        _npcService.SetupRelationship("player1", "npc1", RelationshipLevel.Stranger, 0);

        // Act
        var result = await _sut.GenerateGreetingAsync("player1", "npc1", "Gareth", "merchant");

        // Assert
        result.Should().NotBeNull();
        result.GreetingText.Should().Contain("Greetings, traveler");
        result.GreetingText.Should().Contain("Gareth");
        result.RelationshipLevel.Should().Be(RelationshipLevel.Stranger);
        result.ContextType.Should().Be(GreetingContextType.FirstMeeting);
    }

    [Fact]
    public async Task GenerateGreetingAsync_Acquaintance_ReturnsFriendlierGreeting()
    {
        // Arrange
        _npcService.SetupRelationship("player1", "npc1", RelationshipLevel.Acquaintance, 2);

        // Act
        var result = await _sut.GenerateGreetingAsync("player1", "npc1", "Gareth", "merchant");

        // Assert
        result.GreetingText.Should().Contain("Hello there");
        result.RelationshipLevel.Should().Be(RelationshipLevel.Acquaintance);
    }

    [Fact]
    public async Task GenerateGreetingAsync_Friend_ReturnsWarmGreeting()
    {
        // Arrange
        _npcService.SetupRelationship("player1", "npc1", RelationshipLevel.Friend, 5);

        // Act
        var result = await _sut.GenerateGreetingAsync("player1", "npc1", "Gareth", "merchant");

        // Assert
        result.GreetingText.Should().Contain("friend");
        result.RelationshipLevel.Should().Be(RelationshipLevel.Friend);
    }

    [Fact]
    public async Task GenerateGreetingAsync_TrustedFriend_ReturnsEnthusiasticGreeting()
    {
        // Arrange
        _npcService.SetupRelationship("player1", "npc1", RelationshipLevel.TrustedFriend, 20);

        // Act
        var result = await _sut.GenerateGreetingAsync("player1", "npc1", "Gareth", "merchant");

        // Assert
        result.GreetingText.Should().Contain("trusted");
        result.RelationshipLevel.Should().Be(RelationshipLevel.TrustedFriend);
    }

    [Fact]
    public async Task GenerateGreetingAsync_RecentVisit_AddsContextMessage()
    {
        // Arrange
        _npcService.SetupRelationship("player1", "npc1", RelationshipLevel.Friend, 5);
        AddMemory("player1", "npc1", minutesAgo: 5);

        // Act
        var result = await _sut.GenerateGreetingAsync("player1", "npc1", "Gareth", "merchant");

        // Assert
        result.GreetingText.Should().Contain("Still looking around");
        result.ContextType.Should().Be(GreetingContextType.RecentInteraction);
    }

    [Fact]
    public async Task GenerateGreetingAsync_SameSession_AddsSameSessionMessage()
    {
        // Arrange
        _npcService.SetupRelationship("player1", "npc1", RelationshipLevel.Friend, 5);
        AddMemory("player1", "npc1", minutesAgo: 30);

        // Act
        var result = await _sut.GenerateGreetingAsync("player1", "npc1", "Gareth", "merchant");

        // Assert
        result.GreetingText.Should().Contain("Back so soon");
        result.ContextType.Should().Be(GreetingContextType.SameSession);
    }

    [Fact]
    public async Task GenerateGreetingAsync_LongAbsence_AddsLongAbsenceMessage()
    {
        // Arrange
        _npcService.SetupRelationship("player1", "npc1", RelationshipLevel.Friend, 5);
        AddMemory("player1", "npc1", daysAgo: 10);

        // Act
        var result = await _sut.GenerateGreetingAsync("player1", "npc1", "Gareth", "merchant");

        // Assert
        result.GreetingText.Should().Contain("quite a while");
        result.ContextType.Should().Be(GreetingContextType.LongAbsence);
    }

    [Fact]
    public async Task GenerateGreetingAsync_WithTopics_AddsTopicContext()
    {
        // Arrange
        _npcService.SetupRelationship("player1", "npc1", RelationshipLevel.Friend, 5);
        AddMemoryWithContent("player1", "npc1", "Discussed weapons and armor");
        AddMemoryWithContent("player1", "npc1", "Asked about weapon upgrades");

        // Act
        var result = await _sut.GenerateGreetingAsync("player1", "npc1", "Gareth", "merchant");

        // Assert
        result.GreetingText.Should().Contain("weapons");
        result.RecentTopics.Should().Contain("weapons");
    }

    [Fact]
    public async Task GenerateFarewellAsync_Stranger_ReturnsSimpleFarewell()
    {
        // Arrange
        _npcService.SetupRelationship("player1", "npc1", RelationshipLevel.Stranger, 0);

        // Act
        var result = await _sut.GenerateFarewellAsync("player1", "npc1", purchaseMade: false);

        // Assert
        result.Should().Contain("Safe travels");
    }

    [Fact]
    public async Task GenerateFarewellAsync_Friend_ReturnsWarmFarewell()
    {
        // Arrange
        _npcService.SetupRelationship("player1", "npc1", RelationshipLevel.Friend, 5);

        // Act
        var result = await _sut.GenerateFarewellAsync("player1", "npc1", purchaseMade: false);

        // Assert
        result.Should().Contain("Take care, friend");
    }

    [Fact]
    public async Task GenerateFarewellAsync_WithPurchase_AddsThankYou()
    {
        // Arrange
        _npcService.SetupRelationship("player1", "npc1", RelationshipLevel.Friend, 5);

        // Act
        var result = await _sut.GenerateFarewellAsync("player1", "npc1", purchaseMade: true);

        // Assert
        result.Should().Contain("Thank you for your patronage");
    }

    private void AddMemory(string playerId, string npcId, int minutesAgo = 0, int daysAgo = 0)
    {
        var timestamp = DateTimeOffset.UtcNow.AddMinutes(-minutesAgo).AddDays(-daysAgo);
        var memory = new MemoryEntry
        {
            Id = Guid.NewGuid().ToString(),
            Content = "Previous interaction",
            EntityId = playerId,
            MemoryType = "dialogue_choice",
            Importance = 0.6,
            Timestamp = timestamp,
            Tags = new Dictionary<string, string> { { "npc_id", npcId } }
        };
        _memoryService.StoredMemories.Add(memory);
    }

    private void AddMemoryWithContent(string playerId, string npcId, string content)
    {
        var memory = new MemoryEntry
        {
            Id = Guid.NewGuid().ToString(),
            Content = content,
            EntityId = playerId,
            MemoryType = "dialogue_choice",
            Importance = 0.6,
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-30),
            Tags = new Dictionary<string, string> { { "npc_id", npcId } }
        };
        _memoryService.StoredMemories.Add(memory);
    }

    private class TestMemoryService : IMemoryService
    {
        public List<MemoryEntry> StoredMemories { get; } = new();

        public Task<string> StoreMemoryAsync(MemoryEntry memory, System.Threading.CancellationToken cancellationToken = default)
        {
            StoredMemories.Add(memory);
            return Task.FromResult(memory.Id);
        }

        public Task<IReadOnlyList<MemoryResult>> RetrieveRelevantMemoriesAsync(
            string queryText,
            MemoryRetrievalOptions options,
            System.Threading.CancellationToken cancellationToken = default)
        {
            var results = StoredMemories
                .Where(m => string.IsNullOrEmpty(options.EntityId) || m.EntityId == options.EntityId)
                .Where(m => options.Tags.Count == 0 || options.Tags.All(t => m.Tags.ContainsKey(t.Key) && m.Tags[t.Key] == t.Value))
                .OrderByDescending(m => m.Timestamp)
                .Take(options.Limit)
                .Select(m => new MemoryResult
                {
                    Memory = m,
                    RelevanceScore = 0.9,
                    Source = "test"
                })
                .ToList();

            return Task.FromResult<IReadOnlyList<MemoryResult>>(results);
        }

        public Task<MemoryEntry?> GetMemoryByIdAsync(string memoryId, System.Threading.CancellationToken cancellationToken = default)
        {
            var memory = StoredMemories.FirstOrDefault(m => m.Id == memoryId);
            return Task.FromResult(memory);
        }

        public Task UpdateMemoryImportanceAsync(string memoryId, double importance, System.Threading.CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<int> CleanOldMemoriesAsync(string entityId, TimeSpan olderThan, System.Threading.CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public Task<bool> IsHealthyAsync()
        {
            return Task.FromResult(true);
        }
    }

    private class TestNPCService : MemoryEnhancedNPCService
    {
        private readonly Dictionary<string, NpcRelationshipInsights> _relationships = new();

        public TestNPCService(IMemoryService memoryService)
            : base(null!, null!, null!, NullLogger<MemoryEnhancedNPCService>.Instance)
        {
        }

        public void SetupRelationship(string playerId, string npcId, RelationshipLevel level, int interactions)
        {
            var key = $"{playerId}:{npcId}";
            _relationships[key] = new NpcRelationshipInsights
            {
                NpcId = npcId,
                RelationshipLevel = level,
                InteractionCount = interactions,
                TotalImportance = interactions * 0.6,
                LastInteraction = DateTimeOffset.UtcNow
            };
        }

        public override async Task<NpcRelationshipInsights> GetRelationshipInsightsAsync(string playerId, string npcId)
        {
            var key = $"{playerId}:{npcId}";
            if (_relationships.TryGetValue(key, out var insights))
            {
                return await Task.FromResult(insights);
            }
            return await Task.FromResult(new NpcRelationshipInsights
            {
                NpcId = npcId,
                RelationshipLevel = RelationshipLevel.Stranger,
                InteractionCount = 0,
                TotalImportance = 0
            });
        }

        public override async Task<IReadOnlyList<MemoryResult>> GetRecentDialogueHistoryAsync(string playerId, int limit = 5)
        {
            // Get from the test memory service through base implementation would be complex
            // For now, return empty - the test setup adds memories directly
            return await Task.FromResult<IReadOnlyList<MemoryResult>>(new List<MemoryResult>());
        }
    }
}
