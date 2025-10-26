using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using NSubstitute;
using LablabBean.AI.Agents;
using LablabBean.AI.Agents.Services;
using LablabBean.AI.Agents.Configuration;
using LablabBean.Contracts.AI.Memory;
using LablabBean.AI.Core.Events;
using LablabBean.AI.Core.Models;
using Microsoft.KernelMemory;

namespace LablabBean.AI.Agents.Tests.Integration;

/// <summary>
/// Integration tests for tactical learning workflow
/// Test T058: End-to-end tactical adaptation based on player behavior patterns
/// </summary>
public class TacticalLearningTests
{
    private readonly ILogger<TacticsAgent> _tacticsLogger;
    private readonly ILogger<LablabBean.AI.Agents.Services.MemoryService> _memoryLogger;

    public TacticalLearningTests()
    {
        _tacticsLogger = Substitute.For<ILogger<TacticsAgent>>();
        _memoryLogger = Substitute.For<ILogger<LablabBean.AI.Agents.Services.MemoryService>>();
    }

    [Fact]
    public async Task TacticalLearningLoop_AdaptsToAggressiveRushPattern()
    {
        // Arrange
        var memoryService = CreateMemoryService();
        var tacticsAgent = CreateTacticsAgent(memoryService);
        var playerId = "player_rush";
        var entityId = "enemy_adaptive";

        var context = new AvatarContext
        {
            EntityId = entityId,
            Name = "Adaptive Enemy",
            State = new Dictionary<string, object>
            {
                ["health"] = 80,
                ["maxHealth"] = 100,
                ["location"] = "Arena",
                ["state"] = "Combat"
            }
        };

        // Simulate 5 encounters where player uses aggressive rush
        for (int i = 0; i < 5; i++)
        {
            var observation = new TacticalObservation
            {
                PlayerId = playerId,
                BehaviorType = PlayerBehaviorType.MeleeAggressive,
                EncounterContext = $"Player rushed into melee range aggressively, encounter {i + 1}",
                TacticEffectiveness = new Dictionary<string, float>
                {
                    ["CloseDistance"] = 0.1f,  // Ineffective - player already close
                    ["CutOffEscape"] = 0.7f,   // Somewhat effective
                    ["AggressivePressure"] = 0.5f,  // Neutral - player matches aggression
                    ["DefensiveRetreat"] = 0.2f,  // Failed - player pursues
                    ["Flanking"] = 0.8f,  // Effective - catches player off-guard
                    ["PatternBreak"] = 0.6f  // Somewhat effective
                },
                Outcome = i >= 3 ? OutcomeType.Success : OutcomeType.Failure,
                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-10 * (5 - i))
            };

            await memoryService.StoreTacticalObservationAsync(entityId, observation);
        }

        // Act - Create tactical plan after learning pattern
        var plan = await tacticsAgent.CreateTacticalPlanWithMemoryAsync(
            context,
            playerId,
            memoryService);

        // Assert
        plan.Should().NotBeNull();

        // Enemy should adapt to counter aggressive rush
        // Based on stored observations, Flanking (0.8) was most effective
        var expectedCounterTactics = new[]
        {
            TacticType.Flanking,        // Highest effectiveness (0.8)
            TacticType.CutOffEscape,    // Second highest (0.7)
            TacticType.PatternBreak     // Third highest (0.6)
        };

        expectedCounterTactics.Should().Contain(plan.PrimaryTactic,
            "enemy should select tactics proven effective against aggressive rush");

        // Aggression should be moderate to balanced (not matching player's high aggression)
        plan.Aggression.Should().BeLessThan(0.9f,
            "enemy learned not to match player's aggression");

        // Caution should be moderate (learned from failures)
        plan.Caution.Should().BeGreaterThan(0.3f,
            "enemy learned to be cautious against aggressive rushers");
    }

    [Fact]
    public async Task TacticalLearningLoop_AdaptsToRangedKitingPattern()
    {
        // Arrange
        var memoryService = CreateMemoryService();
        var tacticsAgent = CreateTacticsAgent(memoryService);
        var playerId = "player_kiting";
        var entityId = "enemy_ranged_counter";

        var context = new AvatarContext
        {
            EntityId = entityId,
            Name = "Ranged Counter Enemy",
            State = new Dictionary<string, object>
            {
                ["health"] = 90,
                ["maxHealth"] = 100,
                ["location"] = "Open Field",
                ["state"] = "Combat"
            }
        };

        // Simulate 7 encounters where player uses ranged kiting
        for (int i = 0; i < 7; i++)
        {
            var observation = new TacticalObservation
            {
                PlayerId = playerId,
                BehaviorType = PlayerBehaviorType.Kiting,
                EncounterContext = $"Player maintains distance while attacking with ranged weapons, encounter {i + 1}",
                TacticEffectiveness = new Dictionary<string, float>
                {
                    ["CloseDistance"] = 0.9f,  // Very effective - closes gap
                    ["CutOffEscape"] = 0.85f,  // Effective - limits movement
                    ["AggressivePressure"] = 0.6f,  // Somewhat effective
                    ["DefensiveRetreat"] = 0.1f,  // Failed - player maintains distance
                    ["PatternBreak"] = 0.7f,  // Effective - unpredictable movement
                    ["Flanking"] = 0.75f  // Effective - approaches from sides
                },
                Outcome = i >= 4 ? OutcomeType.Success : OutcomeType.PartialSuccess,
                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-15 * (7 - i))
            };

            await memoryService.StoreTacticalObservationAsync(entityId, observation);
        }

        // Act
        var plan = await tacticsAgent.CreateTacticalPlanWithMemoryAsync(
            context,
            playerId,
            memoryService);

        // Assert
        plan.Should().NotBeNull();

        // Enemy should adapt to counter ranged kiting
        // Based on observations, CloseDistance (0.9), CutOffEscape (0.85), Flanking (0.75) were most effective
        var expectedCounterTactics = new[]
        {
            TacticType.CloseDistance,   // Highest effectiveness
            TacticType.CutOffEscape,    // Second highest
            TacticType.Flanking         // Third highest
        };

        expectedCounterTactics.Should().Contain(plan.PrimaryTactic,
            "enemy should select tactics proven effective against ranged kiting");

        // Aggression should be high (need to close distance aggressively)
        plan.Aggression.Should().BeGreaterThan(0.6f,
            "enemy learned to aggressively close distance against kiters");
    }

    [Fact]
    public async Task TacticalLearningLoop_HandlesNoObservations()
    {
        // Arrange
        var memoryService = CreateMemoryService();
        var tacticsAgent = CreateTacticsAgent(memoryService);
        var playerId = "player_new";
        var entityId = "enemy_no_history";

        var context = new AvatarContext
        {
            EntityId = entityId,
            Name = "Enemy With No History",
            State = new Dictionary<string, object>
            {
                ["health"] = 100,
                ["maxHealth"] = 100,
                ["location"] = "Arena",
                ["state"] = "Combat"
            }
        };

        // Act - No observations stored
        var plan = await tacticsAgent.CreateTacticalPlanWithMemoryAsync(
            context,
            playerId,
            memoryService);

        // Assert
        plan.Should().NotBeNull();
        // Should fall back to default tactics
        plan.PrimaryTactic.Should().NotBe(TacticType.CloseDistance);  // Should get some default
        plan.Confidence.Should().BeLessThan(0.7f, "confidence should be lower without historical data");
    }

    [Fact]
    public async Task TacticalLearningLoop_CrossSessionPersistence()
    {
        // Arrange - Simulate Session 1
        var memoryService = CreateMemoryService();
        var tacticsAgent1 = CreateTacticsAgent(memoryService);
        var playerId = "player_persistent";
        var entityId = "enemy_memory";

        var context = new AvatarContext
        {
            EntityId = entityId,
            Name = "Enemy With Persistent Memory",
            State = new Dictionary<string, object>
            {
                ["health"] = 100,
                ["maxHealth"] = 100,
                ["location"] = "Dungeon",
                ["state"] = "Combat"
            }
        };

        // Session 1: Store observations
        await memoryService.StoreTacticalObservationAsync(entityId, new TacticalObservation
        {
            PlayerId = playerId,
            BehaviorType = PlayerBehaviorType.StatusEffects,
            EncounterContext = "Player frequently uses debuffs and status effects",
            TacticEffectiveness = new Dictionary<string, float>
            {
                ["AggressivePressure"] = 0.9f,  // Interrupt casting
                ["CloseDistance"] = 0.85f  // Close before debuffed
            },
            Outcome = OutcomeType.Success,
            Timestamp = DateTimeOffset.UtcNow.AddHours(-1)
        });

        // Simulate Session 2: New agent instance (simulates restart)
        var tacticsAgent2 = CreateTacticsAgent(memoryService);

        // Act - Retrieve memories from previous session
        var plan = await tacticsAgent2.CreateTacticalPlanWithMemoryAsync(
            context,
            playerId,
            memoryService);

        // Assert
        plan.Should().NotBeNull();

        // Should use learned tactics from previous session
        var expectedTactics = new[] { TacticType.AggressivePressure, TacticType.CloseDistance };
        expectedTactics.Should().Contain(plan.PrimaryTactic,
            "enemy should remember effective tactics from previous session");

        plan.Confidence.Should().BeGreaterThan(0.5f,
            "confidence should be higher with historical data");
    }

    [Fact]
    public async Task TacticalLearningLoop_AggregatesPatterns()
    {
        // Arrange
        var memoryService = CreateMemoryService();
        var tacticsAgent = CreateTacticsAgent(memoryService);
        var playerId = "player_mixed";
        var entityId = "enemy_pattern_analyzer";

        var context = new AvatarContext
        {
            EntityId = entityId,
            Name = "Pattern Analyzing Enemy",
            State = new Dictionary<string, object>
            {
                ["health"] = 100,
                ["maxHealth"] = 100,
                ["location"] = "Battlefield",
                ["state"] = "Combat"
            }
        };

        // Store mixed behaviors - but 80% HitAndRun
        var behaviors = new[]
        {
            PlayerBehaviorType.HitAndRun,
            PlayerBehaviorType.HitAndRun,
            PlayerBehaviorType.HitAndRun,
            PlayerBehaviorType.HitAndRun,
            PlayerBehaviorType.MeleeAggressive,  // Outlier
        };

        foreach (var behavior in behaviors)
        {
            await memoryService.StoreTacticalObservationAsync(entityId, new TacticalObservation
            {
                PlayerId = playerId,
                BehaviorType = behavior,
                EncounterContext = $"Mixed behavior encounter: {behavior}",
                TacticEffectiveness = new Dictionary<string, float>
                {
                    ["CutOffEscape"] = behavior == PlayerBehaviorType.HitAndRun ? 0.9f : 0.3f,
                    ["PatternBreak"] = 0.7f
                },
                Outcome = OutcomeType.PartialSuccess,
                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-behaviors.Length)
            });
        }

        // Act - Query for dominant pattern
        var results = await memoryService.RetrieveSimilarTacticsAsync(
            entityId,
            PlayerBehaviorType.HitAndRun,
            limit: 10);

        // Assert
        results.Should().NotBeEmpty();
        var hitAndRunCount = results.Count(r => r.Memory.Tags["behavior"] == "HitAndRun");
        var totalCount = results.Count;

        // Should retrieve mostly HitAndRun observations
        ((float)hitAndRunCount / totalCount).Should().BeGreaterThan(0.6f,
            "should identify HitAndRun as dominant pattern");
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

        // Setup mock behavior
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

        return new LablabBean.AI.Agents.Services.MemoryService(_memoryLogger, memory, options);
    }

    private TacticsAgent CreateTacticsAgent(IMemoryService memoryService)
    {
        // Create mock kernel with memory service
        var kernelBuilder = Kernel.CreateBuilder();
        // Add mock chat completion service would go here
        var kernel = kernelBuilder.Build();

        return new TacticsAgent(kernel, _tacticsLogger, memoryService);
    }
}

/// <summary>
/// Extension method for TacticsAgent to integrate with MemoryService
/// This will be moved to TacticsAgent.cs in implementation phase
/// </summary>
public static class TacticsAgentMemoryExtensions
{
    public static async Task<TacticalPlan> CreateTacticalPlanWithMemoryAsync(
        this TacticsAgent agent,
        AvatarContext context,
        string playerId,
        IMemoryService memoryService,
        CancellationToken ct = default)
    {
        // This is a placeholder for the integration logic
        // Will be implemented in T062-T066
        // For now, simulate the workflow:

        // 1. Retrieve similar past encounters (T063)
        // 2. Analyze patterns (T065)
        // 3. Select counter-tactics (T064)
        // 4. Execute plan
        // 5. Store observation (T062)

        // Placeholder: Just create default plan
        return new TacticalPlan
        {
            PrimaryTactic = TacticType.AggressivePressure,
            Reasoning = "Integration pending",
            Confidence = 0.5f
        };
    }
}
