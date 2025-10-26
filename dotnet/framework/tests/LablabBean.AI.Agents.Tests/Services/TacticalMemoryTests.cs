using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using LablabBean.AI.Agents.Services;
using LablabBean.AI.Agents.Configuration;
using LablabBean.Contracts.AI.Memory;
using LablabBean.AI.Core.Events;
using Microsoft.KernelMemory;

namespace LablabBean.AI.Agents.Tests.Services;

/// <summary>
/// Unit tests for tactical memory operations in MemoryService
/// Tests T056-T057: Tactical observation storage and retrieval
/// </summary>
public class TacticalMemoryTests
{
    private readonly ILogger<LablabBean.AI.Agents.Services.MemoryService> _logger;

    public TacticalMemoryTests()
    {
        _logger = Substitute.For<ILogger<LablabBean.AI.Agents.Services.MemoryService>>();
    }

    [Fact]
    public async Task StoreTacticalObservationAsync_StoresWithCorrectTags()
    {
        // Arrange
        var memoryService = CreateMemoryService();
        var entityId = "enemy_001";
        var observation = new TacticalObservation
        {
            PlayerId = "player_123",
            BehaviorType = PlayerBehaviorType.MeleeAggressive,
            EncounterContext = "Player rushed into melee range, aggressive stance",
            TacticEffectiveness = new Dictionary<string, float>
            {
                ["CloseDistance"] = 0.2f,  // Failed - player already close
                ["CutOffEscape"] = 0.8f,   // Succeeded - trapped player
                ["AggressivePressure"] = 0.9f  // Succeeded - overwhelmed player
            },
            Outcome = OutcomeType.Success,
            Timestamp = DateTimeOffset.UtcNow
        };

        // Act
        await memoryService.StoreTacticalObservationAsync(entityId, observation);

        // Assert
        var results = await memoryService.RetrieveSimilarTacticsAsync(
            entityId,
            PlayerBehaviorType.MeleeAggressive,
            limit: 10);

        results.Should().NotBeEmpty();
        var stored = results.First();
        stored.Memory.EntityId.Should().Be(entityId);
        stored.Memory.MemoryType.Should().Be("tactical");
        stored.Memory.Tags.Should().ContainKey("behavior");
        stored.Memory.Tags["behavior"].Should().Be("MeleeAggressive");
        stored.Memory.Tags.Should().ContainKey("outcome");
        stored.Memory.Tags["outcome"].Should().Be("Success");
    }

    [Fact]
    public async Task StoreTacticalObservationAsync_StoresMultipleObservations()
    {
        // Arrange
        var memoryService = CreateMemoryService();
        var entityId = "enemy_002";

        var observations = new[]
        {
            new TacticalObservation
            {
                PlayerId = "player_123",
                BehaviorType = PlayerBehaviorType.RangedAttacks,
                EncounterContext = "Player maintains distance with ranged attacks",
                TacticEffectiveness = new Dictionary<string, float>
                {
                    ["CloseDistance"] = 0.7f,
                    ["CutOffEscape"] = 0.3f
                },
                Outcome = OutcomeType.PartialSuccess,
                Timestamp = DateTimeOffset.UtcNow
            },
            new TacticalObservation
            {
                PlayerId = "player_123",
                BehaviorType = PlayerBehaviorType.RangedAttacks,
                EncounterContext = "Player kiting with projectiles",
                TacticEffectiveness = new Dictionary<string, float>
                {
                    ["CloseDistance"] = 0.8f,
                    ["Flanking"] = 0.9f
                },
                Outcome = OutcomeType.Success,
                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-5)
            }
        };

        // Act
        foreach (var observation in observations)
        {
            await memoryService.StoreTacticalObservationAsync(entityId, observation);
        }

        // Assert
        var results = await memoryService.RetrieveSimilarTacticsAsync(
            entityId,
            PlayerBehaviorType.RangedAttacks,
            limit: 10);

        results.Should().HaveCountGreaterOrEqualTo(2);
        results.All(r => r.Memory.Tags["behavior"] == "RangedAttacks").Should().BeTrue();
    }

    [Fact]
    public async Task RetrieveSimilarTacticsAsync_FiltersByBehaviorType()
    {
        // Arrange
        var memoryService = CreateMemoryService();
        var entityId = "enemy_003";

        // Store different behavior types
        await memoryService.StoreTacticalObservationAsync(entityId, new TacticalObservation
        {
            PlayerId = "player_123",
            BehaviorType = PlayerBehaviorType.MeleeAggressive,
            EncounterContext = "Melee encounter",
            TacticEffectiveness = new Dictionary<string, float> { ["AggressivePressure"] = 0.8f },
            Outcome = OutcomeType.Success,
            Timestamp = DateTimeOffset.UtcNow
        });

        await memoryService.StoreTacticalObservationAsync(entityId, new TacticalObservation
        {
            PlayerId = "player_123",
            BehaviorType = PlayerBehaviorType.Defensive,
            EncounterContext = "Defensive encounter",
            TacticEffectiveness = new Dictionary<string, float> { ["AggressivePressure"] = 0.9f },
            Outcome = OutcomeType.Success,
            Timestamp = DateTimeOffset.UtcNow
        });

        // Act
        var meleeResults = await memoryService.RetrieveSimilarTacticsAsync(
            entityId,
            PlayerBehaviorType.MeleeAggressive,
            limit: 10);

        var defensiveResults = await memoryService.RetrieveSimilarTacticsAsync(
            entityId,
            PlayerBehaviorType.Defensive,
            limit: 10);

        // Assert
        meleeResults.Should().NotBeEmpty();
        meleeResults.All(r => r.Memory.Tags["behavior"] == "MeleeAggressive").Should().BeTrue();

        defensiveResults.Should().NotBeEmpty();
        defensiveResults.All(r => r.Memory.Tags["behavior"] == "Defensive").Should().BeTrue();
    }

    [Fact]
    public async Task RetrieveSimilarTacticsAsync_RespectsLimit()
    {
        // Arrange
        var memoryService = CreateMemoryService();
        var entityId = "enemy_004";

        // Store 10 observations
        for (int i = 0; i < 10; i++)
        {
            await memoryService.StoreTacticalObservationAsync(entityId, new TacticalObservation
            {
                PlayerId = "player_123",
                BehaviorType = PlayerBehaviorType.HitAndRun,
                EncounterContext = $"Hit and run encounter {i}",
                TacticEffectiveness = new Dictionary<string, float> { ["CutOffEscape"] = 0.7f },
                Outcome = OutcomeType.PartialSuccess,
                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-i)
            });
        }

        // Act
        var limitedResults = await memoryService.RetrieveSimilarTacticsAsync(
            entityId,
            PlayerBehaviorType.HitAndRun,
            limit: 5);

        // Assert
        limitedResults.Should().HaveCount(5);
    }

    [Fact]
    public async Task RetrieveSimilarTacticsAsync_ReturnsEmptyForUnknownEntity()
    {
        // Arrange
        var memoryService = CreateMemoryService();

        // Act
        var results = await memoryService.RetrieveSimilarTacticsAsync(
            "unknown_entity",
            PlayerBehaviorType.MeleeAggressive,
            limit: 10);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task RetrieveSimilarTacticsAsync_OrdersByRelevance()
    {
        // Arrange
        var memoryService = CreateMemoryService();
        var entityId = "enemy_005";

        // Store observations with varying contexts
        await memoryService.StoreTacticalObservationAsync(entityId, new TacticalObservation
        {
            PlayerId = "player_123",
            BehaviorType = PlayerBehaviorType.AreaOfEffect,
            EncounterContext = "Player uses AOE attacks with fire spells in tight spaces",
            TacticEffectiveness = new Dictionary<string, float> { ["DefensiveRetreat"] = 0.9f },
            Outcome = OutcomeType.Success,
            Timestamp = DateTimeOffset.UtcNow
        });

        await memoryService.StoreTacticalObservationAsync(entityId, new TacticalObservation
        {
            PlayerId = "player_123",
            BehaviorType = PlayerBehaviorType.AreaOfEffect,
            EncounterContext = "Player spamming AOE",
            TacticEffectiveness = new Dictionary<string, float> { ["DefensiveRetreat"] = 0.8f },
            Outcome = OutcomeType.PartialSuccess,
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-5)
        });

        // Act
        var results = await memoryService.RetrieveSimilarTacticsAsync(
            entityId,
            PlayerBehaviorType.AreaOfEffect,
            limit: 10);

        // Assert
        results.Should().NotBeEmpty();
        // First result should have higher relevance than second
        if (results.Count >= 2)
        {
            results[0].RelevanceScore.Should().BeGreaterOrEqualTo(results[1].RelevanceScore);
        }
    }

    private LablabBean.AI.Agents.Services.MemoryService CreateMemoryService()
    {
        // Create mock dependencies for testing
        var memory = Substitute.For<IKernelMemory>();
        var options = Microsoft.Extensions.Options.Options.Create(new KernelMemoryOptions
        {
            Storage = new KernelMemoryOptions.StorageOptions
            {
                Provider = "inmemory",
                CollectionName = "test_memories"
            }
        });

        // Setup mock behavior for search (returns empty results)
        memory.SearchAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<MemoryFilter>(),
            Arg.Any<double>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns(new SearchResult
            {
                Results = new List<Microsoft.KernelMemory.Citation>()
            });

        return new LablabBean.AI.Agents.Services.MemoryService(_logger, memory, options);
    }
}
