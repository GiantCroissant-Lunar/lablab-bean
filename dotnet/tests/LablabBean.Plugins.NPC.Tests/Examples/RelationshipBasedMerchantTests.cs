using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arch.Core;
using Arch.Core.Extensions;
using FluentAssertions;
using LablabBean.Contracts.AI.Memory;
using LablabBean.Plugins.NPC.Examples;
using LablabBean.Plugins.NPC.Services;
using LablabBean.Plugins.NPC.Systems;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LablabBean.Plugins.NPC.Tests.Examples;

public class RelationshipBasedMerchantTests : IDisposable
{
    private readonly World _world;
    private readonly TestMemoryService _memoryService;
    private readonly MemoryEnhancedNPCService _npcService;
    private readonly RelationshipBasedMerchant _sut;
    private readonly MemoryEnhancedDialogueSystem _dialogueSystem;

    public RelationshipBasedMerchantTests()
    {
        _world = World.Create();
        _memoryService = new TestMemoryService();

        var baseDialogueSystem = new DialogueSystem(_world);
        _dialogueSystem = new MemoryEnhancedDialogueSystem(
            baseDialogueSystem,
            _memoryService,
            NullLogger<MemoryEnhancedDialogueSystem>.Instance,
            _world
        );

        var npcSystem = new NPCSystem(_world);
        _npcService = new MemoryEnhancedNPCService(
            _world,
            npcSystem,
            _dialogueSystem,
            NullLogger<MemoryEnhancedNPCService>.Instance
        );

        _sut = new RelationshipBasedMerchant(
            _npcService,
            NullLogger<RelationshipBasedMerchant>.Instance
        );
    }

    [Fact]
    public async Task GetPriceAsync_Stranger_ReturnsFullPrice()
    {
        // Arrange
        const int basePrice = 100;
        SetupRelationship(RelationshipLevel.Stranger, interactionCount: 0);

        // Act
        var result = await _sut.GetPriceAsync("player1", "npc1", basePrice);

        // Assert
        result.Should().Be(100); // 0% discount
    }

    [Fact]
    public async Task GetPriceAsync_Acquaintance_Returns5PercentDiscount()
    {
        // Arrange
        const int basePrice = 100;
        SetupRelationship(RelationshipLevel.Acquaintance, interactionCount: 2);

        // Act
        var result = await _sut.GetPriceAsync("player1", "npc1", basePrice);

        // Assert
        result.Should().Be(95); // 5% discount
    }

    [Fact]
    public async Task GetPriceAsync_Friend_Returns10PercentDiscount()
    {
        // Arrange
        const int basePrice = 100;
        SetupRelationship(RelationshipLevel.Friend, interactionCount: 5);

        // Act
        var result = await _sut.GetPriceAsync("player1", "npc1", basePrice);

        // Assert
        result.Should().Be(90); // 10% discount
    }

    [Fact]
    public async Task GetPriceAsync_GoodFriend_Returns15PercentDiscount()
    {
        // Arrange
        const int basePrice = 100;
        SetupRelationship(RelationshipLevel.GoodFriend, interactionCount: 8);

        // Act
        var result = await _sut.GetPriceAsync("player1", "npc1", basePrice);

        // Assert
        result.Should().Be(85); // 15% discount
    }

    [Fact]
    public async Task GetPriceAsync_CloseFriend_Returns20PercentDiscount()
    {
        // Arrange
        const int basePrice = 100;
        SetupRelationship(RelationshipLevel.CloseFriend, interactionCount: 12);

        // Act
        var result = await _sut.GetPriceAsync("player1", "npc1", basePrice);

        // Assert
        result.Should().Be(80); // 20% discount
    }

    [Fact]
    public async Task GetPriceAsync_TrustedFriend_Returns25PercentDiscount()
    {
        // Arrange
        const int basePrice = 100;
        SetupRelationship(RelationshipLevel.TrustedFriend, interactionCount: 20);

        // Act
        var result = await _sut.GetPriceAsync("player1", "npc1", basePrice);

        // Assert
        result.Should().Be(75); // 25% discount
    }

    [Theory]
    [InlineData(50, 0, 50)]      // Stranger: no discount
    [InlineData(50, 5, 47)]      // Acquaintance: 5% off (rounds to 47)
    [InlineData(50, 10, 45)]     // Friend: 10% off
    [InlineData(50, 15, 42)]     // GoodFriend: 15% off (rounds to 42)
    [InlineData(50, 20, 40)]     // CloseFriend: 20% off
    [InlineData(50, 25, 37)]     // TrustedFriend: 25% off (rounds to 37)
    public async Task GetPriceAsync_VariousPrices_ReturnsCorrectDiscount(
        int basePrice,
        int discountPercent,
        int expectedPrice)
    {
        // Arrange
        var level = discountPercent switch
        {
            0 => RelationshipLevel.Stranger,
            5 => RelationshipLevel.Acquaintance,
            10 => RelationshipLevel.Friend,
            15 => RelationshipLevel.GoodFriend,
            20 => RelationshipLevel.CloseFriend,
            25 => RelationshipLevel.TrustedFriend,
            _ => RelationshipLevel.Stranger
        };
        SetupRelationship(level, interactionCount: 1);

        // Act
        var result = await _sut.GetPriceAsync("player1", "npc1", basePrice);

        // Assert
        result.Should().Be(expectedPrice);
    }

    [Fact]
    public async Task CanAccessExclusiveItemsAsync_Stranger_ReturnsFalse()
    {
        // Arrange
        SetupRelationship(RelationshipLevel.Stranger, interactionCount: 0);

        // Act
        var result = await _sut.CanAccessExclusiveItemsAsync("player1", "npc1");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanAccessExclusiveItemsAsync_Friend_ReturnsFalse()
    {
        // Arrange
        SetupRelationship(RelationshipLevel.Friend, interactionCount: 5);

        // Act
        var result = await _sut.CanAccessExclusiveItemsAsync("player1", "npc1");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanAccessExclusiveItemsAsync_GoodFriend_ReturnsTrue()
    {
        // Arrange
        SetupRelationship(RelationshipLevel.GoodFriend, interactionCount: 8);

        // Act
        var result = await _sut.CanAccessExclusiveItemsAsync("player1", "npc1");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanAccessExclusiveItemsAsync_TrustedFriend_ReturnsTrue()
    {
        // Arrange
        SetupRelationship(RelationshipLevel.TrustedFriend, interactionCount: 20);

        // Act
        var result = await _sut.CanAccessExclusiveItemsAsync("player1", "npc1");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanAccessExclusiveItemsAsync_CustomMinimumLevel_RespectsThreshold()
    {
        // Arrange
        SetupRelationship(RelationshipLevel.Friend, interactionCount: 5);

        // Act
        var result = await _sut.CanAccessExclusiveItemsAsync(
            "player1",
            "npc1",
            minimumLevel: RelationshipLevel.Friend
        );

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetPersonalizedGreetingAsync_Stranger_ReturnsGenericGreeting()
    {
        // Arrange
        SetupRelationship(RelationshipLevel.Stranger, interactionCount: 0);

        // Act
        var result = await _sut.GetPersonalizedGreetingAsync("player1", "npc1", "Gareth");

        // Assert
        result.Should().Contain("Welcome");
        result.Should().Contain("Gareth");
    }

    [Fact]
    public async Task GetPersonalizedGreetingAsync_Friend_ReturnsFriendlyGreeting()
    {
        // Arrange
        SetupRelationship(RelationshipLevel.Friend, interactionCount: 5);

        // Act
        var result = await _sut.GetPersonalizedGreetingAsync("player1", "npc1", "Gareth");

        // Assert
        result.Should().Contain("friend");
    }

    [Fact]
    public async Task GetPersonalizedGreetingAsync_RecentVisit_AddsContextualMessage()
    {
        // Arrange
        SetupRelationship(RelationshipLevel.Friend, interactionCount: 5);
        AddRecentMemory(minutesAgo: 10);

        // Act
        var result = await _sut.GetPersonalizedGreetingAsync("player1", "npc1", "Gareth");

        // Assert
        result.Should().Contain("Back already");
    }

    [Fact]
    public async Task HandlePurchaseAsync_CompletesFullTransaction()
    {
        // Arrange
        var player = _world.Create();
        var merchant = _world.Create(new Components.NPC
        {
            Id = "merchant1",
            Name = "Gareth"
        });
        SetupRelationship(RelationshipLevel.Friend, interactionCount: 5);

        // Act
        var result = await _sut.HandlePurchaseAsync(
            player,
            merchant,
            "player1",
            "Health Potion",
            100
        );

        // Assert
        result.Should().NotBeNull();
        result.ItemName.Should().Be("Health Potion");
        result.BasePrice.Should().Be(100);
        result.FinalPrice.Should().Be(90);
        result.DiscountPercent.Should().Be(10);
        result.RelationshipLevel.Should().Be(RelationshipLevel.Friend);
        result.InteractionCount.Should().Be(5);
        result.Greeting.Should().NotBeNullOrEmpty();
    }

    private void SetupRelationship(RelationshipLevel level, int interactionCount)
    {
        // Create memories that will result in the desired relationship level
        double importance = level switch
        {
            RelationshipLevel.Stranger => 0.2,
            RelationshipLevel.Acquaintance => 0.5,
            RelationshipLevel.Friend => 0.6,
            RelationshipLevel.GoodFriend => 0.65,
            RelationshipLevel.CloseFriend => 0.7,
            RelationshipLevel.TrustedFriend => 0.75,
            _ => 0.5
        };

        for (int i = 0; i < interactionCount; i++)
        {
            var memory = new MemoryEntry
            {
                Id = Guid.NewGuid().ToString(),
                Content = $"Interaction {i + 1}",
                EntityId = "player1",
                MemoryType = "dialogue_choice",
                Importance = importance,
                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-i * 10),
                Tags = new Dictionary<string, string>
                {
                    { "npc_id", "npc1" }
                }
            };
            _memoryService.StoredMemories.Add(memory);
        }
    }

    private void AddRecentMemory(int minutesAgo)
    {
        var memory = new MemoryEntry
        {
            Id = Guid.NewGuid().ToString(),
            Content = "Previous interaction",
            EntityId = "player1",
            MemoryType = "dialogue_choice",
            Importance = 0.6,
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-minutesAgo),
            Tags = new Dictionary<string, string>
            {
                { "npc_id", "npc1" }
            }
        };
        _memoryService.StoredMemories.Add(memory);
    }

    public void Dispose()
    {
        _world.Dispose();
        GC.SuppressFinalize(this);
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
}
