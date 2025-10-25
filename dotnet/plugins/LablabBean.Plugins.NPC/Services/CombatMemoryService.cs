using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.NPC.Components;
using LablabBean.Plugins.NPC.Systems;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.NPC.Services;

/// <summary>
/// Service interface for managing combat memory and analytics.
/// </summary>
public interface ICombatMemoryService
{
    /// <summary>
    /// Gets combat history between an NPC and an opponent.
    /// </summary>
    Task<CombatHistory?> GetCombatHistoryAsync(Guid npcId, Guid opponentId);

    /// <summary>
    /// Records a combat encounter for an NPC.
    /// </summary>
    Task RecordCombatAsync(Guid npcId, Guid opponentId, CombatEncounter encounter);

    /// <summary>
    /// Gets comprehensive combat analytics for an NPC.
    /// </summary>
    Task<CombatAnalytics> GetCombatAnalyticsAsync(Guid npcId);

    /// <summary>
    /// Gets recent combat encounters for an NPC.
    /// </summary>
    Task<List<CombatEncounter>> GetRecentCombatsAsync(Guid npcId, int count = 10);

    /// <summary>
    /// Gets the combat relationship between an NPC and opponent.
    /// </summary>
    Task<CombatRelationship> GetCombatRelationshipAsync(Guid npcId, Guid opponentId);

    /// <summary>
    /// Gets tactical analysis for an NPC's tactics.
    /// </summary>
    Task<Dictionary<string, TacticAnalysis>> GetTacticAnalysisAsync(Guid npcId);

    /// <summary>
    /// Gets the optimal counter-tactic for an opponent's tactic.
    /// </summary>
    Task<string?> GetOptimalCounterTacticAsync(Guid npcId, string playerTactic);

    /// <summary>
    /// Gets adapted behavior for an NPC against an opponent.
    /// </summary>
    Task<AdaptedBehavior> GetAdaptedBehaviorAsync(Guid npcId, Guid opponentId);
}

/// <summary>
/// Service for managing combat memory, analytics, and adaptive tactics.
/// </summary>
public class CombatMemoryService : ICombatMemoryService
{
    private readonly World _world;
    private readonly CombatMemorySystem _combatMemorySystem;
    private readonly AdaptiveTacticsSystem _adaptiveTacticsSystem;
    private readonly ILogger<CombatMemoryService> _logger;

    public CombatMemoryService(
        World world,
        CombatMemorySystem combatMemorySystem,
        AdaptiveTacticsSystem adaptiveTacticsSystem,
        ILogger<CombatMemoryService> logger)
    {
        _world = world;
        _combatMemorySystem = combatMemorySystem;
        _adaptiveTacticsSystem = adaptiveTacticsSystem;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<CombatHistory?> GetCombatHistoryAsync(Guid npcId, Guid opponentId)
    {
        try
        {
            var npcEntity = FindEntityById(npcId);
            var opponentEntity = FindEntityById(opponentId);

            if (npcEntity == null || opponentEntity == null)
            {
                _logger.LogWarning("Entity not found: NPC={NpcId}, Opponent={OpponentId}", npcId, opponentId);
                return Task.FromResult<CombatHistory?>(null);
            }

            var history = _combatMemorySystem.GetCombatHistory(npcEntity.Value, opponentEntity.Value);
            return Task.FromResult(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting combat history for NPC {NpcId} vs {OpponentId}", npcId, opponentId);
            return Task.FromResult<CombatHistory?>(null);
        }
    }

    /// <inheritdoc/>
    public Task RecordCombatAsync(Guid npcId, Guid opponentId, CombatEncounter encounter)
    {
        try
        {
            var npcEntity = FindEntityById(npcId);
            var opponentEntity = FindEntityById(opponentId);

            if (npcEntity == null || opponentEntity == null)
            {
                _logger.LogWarning("Entity not found when recording combat: NPC={NpcId}, Opponent={OpponentId}",
                    npcId, opponentId);
                return Task.CompletedTask;
            }

            // Manually record the encounter (for external combat systems)
            if (!npcEntity.Value.Has<CombatMemory>())
            {
                npcEntity.Value.Add(CombatMemory.Create());
            }

            ref var memory = ref npcEntity.Value.Get<CombatMemory>();
            var history = memory.GetOrCreateHistory(opponentId);
            history.Encounters.Add(encounter);
            history.LastFight = DateTime.UtcNow;

            // Update stats
            switch (encounter.Outcome)
            {
                case CombatOutcome.Win:
                    history.Wins++;
                    memory.Wins++;
                    break;
                case CombatOutcome.Loss:
                    history.Losses++;
                    memory.Losses++;
                    break;
                default:
                    history.Draws++;
                    memory.Draws++;
                    break;
            }

            memory.TotalCombats++;
            memory.TotalDamageDealt += encounter.DamageDealt;
            memory.TotalDamageReceived += encounter.DamageReceived;
            memory.LastCombatTime = DateTime.UtcNow;

            history.UpdateRelationship();
            memory.Encounters[opponentId] = history;

            npcEntity.Value.Set(memory);

            _logger.LogInformation("Recorded combat for NPC {NpcId} vs {OpponentId}: {Outcome}",
                npcId, opponentId, encounter.Outcome);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording combat for NPC {NpcId} vs {OpponentId}", npcId, opponentId);
            return Task.CompletedTask;
        }
    }

    /// <inheritdoc/>
    public Task<CombatAnalytics> GetCombatAnalyticsAsync(Guid npcId)
    {
        try
        {
            var npcEntity = FindEntityById(npcId);

            if (npcEntity == null || !npcEntity.Value.Has<CombatMemory>())
            {
                return Task.FromResult(new CombatAnalytics
                {
                    TotalCombats = 0,
                    Wins = 0,
                    Losses = 0,
                    Draws = 0,
                    WinRate = 0.0,
                    TotalDamageDealt = 0,
                    TotalDamageReceived = 0,
                    AverageCombatDuration = TimeSpan.Zero,
                    TacticUsageCount = new Dictionary<string, int>(),
                    Relationships = new Dictionary<Guid, CombatRelationship>()
                });
            }

            var memory = npcEntity.Value.Get<CombatMemory>();

            // Calculate average combat duration
            var allEncounters = memory.Encounters?.Values
                .SelectMany(h => h.Encounters)
                .ToList() ?? new List<CombatEncounter>();

            var avgDuration = allEncounters.Count > 0
                ? TimeSpan.FromTicks((long)allEncounters.Average(e => e.Duration.Ticks))
                : TimeSpan.Zero;

            // Count tactic usage
            var tacticCounts = new Dictionary<string, int>();
            foreach (var encounter in allEncounters)
            {
                foreach (var tactic in encounter.TacticsUsed)
                {
                    tacticCounts[tactic] = tacticCounts.GetValueOrDefault(tactic, 0) + 1;
                }
            }

            // Build relationships dictionary
            var relationships = memory.Encounters?.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Relationship
            ) ?? new Dictionary<Guid, CombatRelationship>();

            var analytics = new CombatAnalytics
            {
                TotalCombats = memory.TotalCombats,
                Wins = memory.Wins,
                Losses = memory.Losses,
                Draws = memory.Draws,
                WinRate = memory.GetWinRate(),
                TotalDamageDealt = memory.TotalDamageDealt,
                TotalDamageReceived = memory.TotalDamageReceived,
                AverageCombatDuration = avgDuration,
                TacticUsageCount = tacticCounts,
                Relationships = relationships
            };

            return Task.FromResult(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting combat analytics for NPC {NpcId}", npcId);
            return Task.FromResult(new CombatAnalytics
            {
                TotalCombats = 0,
                Wins = 0,
                Losses = 0,
                Draws = 0,
                WinRate = 0.0,
                TotalDamageDealt = 0,
                TotalDamageReceived = 0,
                AverageCombatDuration = TimeSpan.Zero,
                TacticUsageCount = new Dictionary<string, int>(),
                Relationships = new Dictionary<Guid, CombatRelationship>()
            });
        }
    }

    /// <inheritdoc/>
    public Task<List<CombatEncounter>> GetRecentCombatsAsync(Guid npcId, int count = 10)
    {
        try
        {
            var npcEntity = FindEntityById(npcId);

            if (npcEntity == null || !npcEntity.Value.Has<CombatMemory>())
            {
                return Task.FromResult(new List<CombatEncounter>());
            }

            var memory = npcEntity.Value.Get<CombatMemory>();

            var recentEncounters = memory.Encounters?.Values
                .SelectMany(h => h.Encounters)
                .OrderByDescending(e => e.Timestamp)
                .Take(count)
                .ToList() ?? new List<CombatEncounter>();

            return Task.FromResult(recentEncounters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent combats for NPC {NpcId}", npcId);
            return Task.FromResult(new List<CombatEncounter>());
        }
    }

    /// <inheritdoc/>
    public Task<CombatRelationship> GetCombatRelationshipAsync(Guid npcId, Guid opponentId)
    {
        try
        {
            var npcEntity = FindEntityById(npcId);
            var opponentEntity = FindEntityById(opponentId);

            if (npcEntity == null || opponentEntity == null)
            {
                return Task.FromResult(CombatRelationship.Neutral);
            }

            var relationship = _combatMemorySystem.GetCombatRelationship(npcEntity.Value, opponentEntity.Value);
            return Task.FromResult(relationship);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting combat relationship for NPC {NpcId} vs {OpponentId}",
                npcId, opponentId);
            return Task.FromResult(CombatRelationship.Neutral);
        }
    }

    /// <inheritdoc/>
    public Task<Dictionary<string, TacticAnalysis>> GetTacticAnalysisAsync(Guid npcId)
    {
        try
        {
            var npcEntity = FindEntityById(npcId);

            if (npcEntity == null || !npcEntity.Value.Has<TacticalMemory>())
            {
                return Task.FromResult(new Dictionary<string, TacticAnalysis>());
            }

            var tactical = npcEntity.Value.Get<TacticalMemory>();
            return Task.FromResult(tactical.LearnedTactics ?? new Dictionary<string, TacticAnalysis>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tactic analysis for NPC {NpcId}", npcId);
            return Task.FromResult(new Dictionary<string, TacticAnalysis>());
        }
    }

    /// <inheritdoc/>
    public Task<string?> GetOptimalCounterTacticAsync(Guid npcId, string playerTactic)
    {
        try
        {
            var npcEntity = FindEntityById(npcId);

            if (npcEntity == null || !npcEntity.Value.Has<TacticalMemory>())
            {
                return Task.FromResult<string?>(null);
            }

            var tactical = npcEntity.Value.Get<TacticalMemory>();
            var counter = tactical.GetBestCounter(playerTactic);

            return Task.FromResult(counter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting optimal counter for NPC {NpcId} vs tactic {Tactic}",
                npcId, playerTactic);
            return Task.FromResult<string?>(null);
        }
    }

    /// <inheritdoc/>
    public Task<AdaptedBehavior> GetAdaptedBehaviorAsync(Guid npcId, Guid opponentId)
    {
        try
        {
            var npcEntity = FindEntityById(npcId);
            var opponentEntity = FindEntityById(opponentId);

            if (npcEntity == null || opponentEntity == null)
            {
                return Task.FromResult(new AdaptedBehavior
                {
                    TacticCategory = TacticCategory.Aggressive,
                    SelectedTactic = "Basic Attack",
                    Aggression = 0.5,
                    CautiousLevel = 0.5,
                    ShouldFlee = false
                });
            }

            var behavior = _adaptiveTacticsSystem.GetAdaptedBehavior(npcEntity.Value, opponentEntity.Value);
            return Task.FromResult(behavior);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting adapted behavior for NPC {NpcId} vs {OpponentId}",
                npcId, opponentId);
            return Task.FromResult(new AdaptedBehavior
            {
                TacticCategory = TacticCategory.Aggressive,
                SelectedTactic = "Basic Attack",
                Aggression = 0.5,
                CautiousLevel = 0.5,
                ShouldFlee = false
            });
        }
    }

    /// <summary>
    /// Finds an entity by its ID.
    /// Note: This is a simplified implementation. In production, use proper entity lookup.
    /// </summary>
    private Entity? FindEntityById(Guid id)
    {
        Entity? found = null;
        var query = new QueryDescription().WithAll<Components.NPC>();

        _world.Query(in query, (Entity entity) =>
        {
            if (entity.Id == (int)id.GetHashCode()) // Simplified ID matching
            {
                found = entity;
            }
        });

        return found;
    }
}

/// <summary>
/// Comprehensive combat analytics for an NPC.
/// </summary>
public class CombatAnalytics
{
    /// <summary>
    /// Total number of combats participated in.
    /// </summary>
    public required int TotalCombats { get; set; }

    /// <summary>
    /// Total victories.
    /// </summary>
    public required int Wins { get; set; }

    /// <summary>
    /// Total defeats.
    /// </summary>
    public required int Losses { get; set; }

    /// <summary>
    /// Total draws/fled combats.
    /// </summary>
    public required int Draws { get; set; }

    /// <summary>
    /// Win rate (0.0 to 1.0).
    /// </summary>
    public required double WinRate { get; set; }

    /// <summary>
    /// Total damage dealt across all combats.
    /// </summary>
    public required int TotalDamageDealt { get; set; }

    /// <summary>
    /// Total damage received across all combats.
    /// </summary>
    public required int TotalDamageReceived { get; set; }

    /// <summary>
    /// Average duration of combat encounters.
    /// </summary>
    public required TimeSpan AverageCombatDuration { get; set; }

    /// <summary>
    /// Count of tactic usage (tactic name -> count).
    /// </summary>
    public required Dictionary<string, int> TacticUsageCount { get; set; }

    /// <summary>
    /// Combat relationships with all opponents.
    /// </summary>
    public required Dictionary<Guid, CombatRelationship> Relationships { get; set; }
}
