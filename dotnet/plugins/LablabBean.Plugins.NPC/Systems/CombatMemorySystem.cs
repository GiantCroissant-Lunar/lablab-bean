using System;
using System.Linq;
using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.NPC.Components;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.NPC.Systems;

/// <summary>
/// Tracks combat encounters, updates combat memory, and manages combat-based relationships.
/// Records damage, outcomes, and tactics to enable adaptive AI behavior.
/// </summary>
public class CombatMemorySystem
{
    private readonly World _world;
    private readonly ILogger<CombatMemorySystem> _logger;

    // Active combat tracking
    private readonly Dictionary<Guid, ActiveCombat> _activeCombats = new();

    public CombatMemorySystem(World world, ILogger<CombatMemorySystem> logger)
    {
        _world = world;
        _logger = logger;
    }

    /// <summary>
    /// Called when combat starts between two entities.
    /// </summary>
    public void OnCombatStart(Entity attacker, Entity defender, string? combatId = null)
    {
        try
        {
            // Generate combat ID if not provided
            combatId ??= Guid.NewGuid().ToString();

            // Ensure entities have combat memory components
            EnsureCombatMemory(attacker);
            EnsureCombatMemory(defender);

            EnsureTacticalMemory(attacker);
            EnsureTacticalMemory(defender);

            // Track active combat
            var activeCombat = new ActiveCombat
            {
                CombatId = combatId,
                Attacker = attacker,
                Defender = defender,
                StartTime = DateTime.UtcNow,
                AttackerDamageDealt = 0,
                DefenderDamageDealt = 0,
                AttackerTactics = new List<string>(),
                DefenderTactics = new List<string>(),
                TurnCount = 0
            };

            _activeCombats[Guid.Parse(combatId)] = activeCombat;

            _logger.LogInformation("Combat started: {CombatId} between {Attacker} and {Defender}",
                combatId, attacker.Id, defender.Id);

            // Update emotional states based on history
            UpdateEmotionalStateOnCombatStart(attacker, defender);
            UpdateEmotionalStateOnCombatStart(defender, attacker);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting combat between {Attacker} and {Defender}",
                attacker.Id, defender.Id);
        }
    }

    /// <summary>
    /// Called when damage is dealt during combat.
    /// </summary>
    public void OnDamageDealt(Entity attacker, Entity target, int damage, string? tacticUsed = null)
    {
        try
        {
            // Find active combat
            var activeCombat = _activeCombats.Values
                .FirstOrDefault(c => (c.Attacker == attacker && c.Defender == target) ||
                                    (c.Attacker == target && c.Defender == attacker));

            if (activeCombat == null)
            {
                _logger.LogWarning("Damage dealt outside of tracked combat: {Attacker} -> {Target}",
                    attacker.Id, target.Id);
                return;
            }

            // Record damage
            if (activeCombat.Attacker == attacker)
            {
                activeCombat.AttackerDamageDealt += damage;
                if (!string.IsNullOrEmpty(tacticUsed))
                {
                    activeCombat.AttackerTactics.Add(tacticUsed);
                }
            }
            else
            {
                activeCombat.DefenderDamageDealt += damage;
                if (!string.IsNullOrEmpty(tacticUsed))
                {
                    activeCombat.DefenderTactics.Add(tacticUsed);
                }
            }

            activeCombat.TurnCount++;

            _logger.LogDebug("Damage recorded: {Attacker} dealt {Damage} to {Target} using {Tactic}",
                attacker.Id, damage, target.Id, tacticUsed ?? "basic attack");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording damage: {Attacker} -> {Target}",
                attacker.Id, target.Id);
        }
    }

    /// <summary>
    /// Called when combat ends.
    /// </summary>
    public void OnCombatEnd(Entity winner, Entity loser, CombatOutcome outcome)
    {
        try
        {
            // Find and remove active combat
            var combatPair = _activeCombats
                .FirstOrDefault(kvp => (kvp.Value.Attacker == winner && kvp.Value.Defender == loser) ||
                                      (kvp.Value.Attacker == loser && kvp.Value.Defender == winner));

            if (combatPair.Value == null)
            {
                _logger.LogWarning("Combat ended but not found in active combats: {Winner} vs {Loser}",
                    winner.Id, loser.Id);
                return;
            }

            var activeCombat = combatPair.Value;
            _activeCombats.Remove(combatPair.Key);

            // Determine actual attacker/defender
            bool winnerWasAttacker = activeCombat.Attacker == winner;

            // Create encounter records
            var duration = DateTime.UtcNow - activeCombat.StartTime;

            // Winner's encounter
            var winnerEncounter = new CombatEncounter
            {
                Timestamp = DateTime.UtcNow,
                Outcome = CombatOutcome.Win,
                DamageDealt = winnerWasAttacker ? activeCombat.AttackerDamageDealt : activeCombat.DefenderDamageDealt,
                DamageReceived = winnerWasAttacker ? activeCombat.DefenderDamageDealt : activeCombat.AttackerDamageDealt,
                TacticsUsed = winnerWasAttacker ? activeCombat.AttackerTactics : activeCombat.DefenderTactics,
                OpponentTactics = winnerWasAttacker ? activeCombat.DefenderTactics : activeCombat.AttackerTactics,
                Duration = duration,
                TurnsToComplete = activeCombat.TurnCount,
                EndingHealthPercent = 0.5 // Placeholder - should be calculated from actual health
            };

            // Loser's encounter
            var loserEncounter = new CombatEncounter
            {
                Timestamp = DateTime.UtcNow,
                Outcome = outcome,
                DamageDealt = winnerWasAttacker ? activeCombat.DefenderDamageDealt : activeCombat.AttackerDamageDealt,
                DamageReceived = winnerWasAttacker ? activeCombat.AttackerDamageDealt : activeCombat.DefenderDamageDealt,
                TacticsUsed = winnerWasAttacker ? activeCombat.DefenderTactics : activeCombat.AttackerTactics,
                OpponentTactics = winnerWasAttacker ? activeCombat.AttackerTactics : activeCombat.DefenderTactics,
                Duration = duration,
                TurnsToComplete = activeCombat.TurnCount,
                EndingHealthPercent = 0.1 // Placeholder
            };

            // Update combat memories
            RecordEncounter(winner, loser, winnerEncounter);
            RecordEncounter(loser, winner, loserEncounter);

            // Update tactical memories
            UpdateTacticalMemories(winner, loser, winnerEncounter);
            UpdateTacticalMemories(loser, winner, loserEncounter);

            _logger.LogInformation("Combat ended: {Winner} defeated {Loser} ({Outcome})",
                winner.Id, loser.Id, outcome);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending combat: {Winner} vs {Loser}", winner.Id, loser.Id);
        }
    }

    /// <summary>
    /// Records a combat encounter in an entity's combat memory.
    /// </summary>
    private void RecordEncounter(Entity entity, Entity opponent, CombatEncounter encounter)
    {
        if (!entity.Has<CombatMemory>())
        {
            _logger.LogWarning("Entity {EntityId} missing CombatMemory component", entity.Id);
            return;
        }

        ref var memory = ref entity.Get<CombatMemory>();

        // Get or create history with opponent
        var opponentKey = GetEntityGuid(opponent);
        var history = memory.GetOrCreateHistory(opponentKey);
        history.Encounters.Add(encounter);
        history.LastFight = DateTime.UtcNow;

        // Update win/loss counters
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
            case CombatOutcome.Fled:
            case CombatOutcome.Draw:
                history.Draws++;
                memory.Draws++;
                break;
        }

        // Update totals
        memory.TotalCombats++;
        memory.TotalDamageDealt += encounter.DamageDealt;
        memory.TotalDamageReceived += encounter.DamageReceived;
        memory.LastCombatTime = DateTime.UtcNow;

        // Update relationship
        history.UpdateRelationship();
        memory.Encounters[opponentKey] = history;

        // Update emotional state based on recent performance
        UpdateEmotionalState(ref memory, history);

        entity.Set(memory);
    }

    /// <summary>
    /// Updates tactical memories based on combat encounter.
    /// </summary>
    private void UpdateTacticalMemories(Entity entity, Entity opponent, CombatEncounter encounter)
    {
        if (!entity.Has<TacticalMemory>())
        {
            _logger.LogWarning("Entity {EntityId} missing TacticalMemory component", entity.Id);
            return;
        }

        ref var tactical = ref entity.Get<TacticalMemory>();

        bool isWin = encounter.Outcome == CombatOutcome.Win;

        // Record usage of own tactics
        foreach (var tactic in encounter.TacticsUsed)
        {
            double damagePerUse = encounter.TacticsUsed.Count > 0
                ? (double)encounter.DamageDealt / encounter.TacticsUsed.Count
                : 0;
            tactical.RecordTacticUsage(tactic, isWin, damagePerUse);
        }

        // Record counters (what tactics worked against opponent's tactics)
        if (isWin)
        {
            foreach (var opponentTactic in encounter.OpponentTactics)
            {
                foreach (var counterTactic in encounter.TacticsUsed)
                {
                    tactical.RecordCounter(opponentTactic, counterTactic, true);
                }
            }
        }

        // Update preferred tactic periodically
        if (DateTime.UtcNow - tactical.LastAnalysis > TimeSpan.FromMinutes(5))
        {
            tactical.UpdatePreferredTactic();
        }

        entity.Set(tactical);
    }

    /// <summary>
    /// Updates emotional state based on combat history.
    /// </summary>
    private void UpdateEmotionalState(ref CombatMemory memory, CombatHistory history)
    {
        // Calculate recent performance (last 3 fights)
        var recentFights = history.Encounters.TakeLast(3).ToList();
        if (recentFights.Count == 0)
        {
            memory.EmotionalState = CombatEmotionalState.Neutral;
            return;
        }

        int recentWins = recentFights.Count(e => e.Outcome == CombatOutcome.Win);
        int recentLosses = recentFights.Count(e => e.Outcome == CombatOutcome.Loss);

        // Determine emotional state
        if (history.Relationship == CombatRelationship.Afraid)
        {
            memory.EmotionalState = CombatEmotionalState.Afraid;
        }
        else if (history.Relationship == CombatRelationship.Hunter || (recentLosses > recentWins && history.Wins > 0))
        {
            memory.EmotionalState = CombatEmotionalState.Vengeful;
        }
        else if (recentWins >= 2)
        {
            memory.EmotionalState = CombatEmotionalState.Confident;
        }
        else if (recentLosses >= 2)
        {
            memory.EmotionalState = CombatEmotionalState.Desperate;
        }
        else
        {
            memory.EmotionalState = CombatEmotionalState.Cautious;
        }
    }

    /// <summary>
    /// Updates emotional state when combat starts (based on history with opponent).
    /// </summary>
    private void UpdateEmotionalStateOnCombatStart(Entity entity, Entity opponent)
    {
        if (!entity.Has<CombatMemory>())
            return;

        ref var memory = ref entity.Get<CombatMemory>();

        var opponentKey = GetEntityGuid(opponent);
        if (memory.Encounters != null && memory.Encounters.TryGetValue(opponentKey, out var history))
        {
            UpdateEmotionalState(ref memory, history);
            entity.Set(memory);
        }
    }

    /// <summary>
    /// Ensures an entity has a CombatMemory component.
    /// </summary>
    private void EnsureCombatMemory(Entity entity)
    {
        if (!entity.Has<CombatMemory>())
        {
            entity.Add(CombatMemory.Create());
        }
    }

    /// <summary>
    /// Ensures an entity has a TacticalMemory component.
    /// </summary>
    private void EnsureTacticalMemory(Entity entity)
    {
        if (!entity.Has<TacticalMemory>())
        {
            entity.Add(TacticalMemory.Create());
        }
    }

    /// <summary>
    /// Gets combat history between two entities.
    /// </summary>
    public CombatHistory? GetCombatHistory(Entity entity, Entity opponent)
    {
        if (!entity.Has<CombatMemory>())
            return null;

        var memory = entity.Get<CombatMemory>();
        var opponentKey = GetEntityGuid(opponent);
        return memory.Encounters?.TryGetValue(opponentKey, out var history) == true ? history : null;
    }

    /// <summary>
    /// Gets the combat relationship between two entities.
    /// </summary>
    public CombatRelationship GetCombatRelationship(Entity entity, Entity opponent)
    {
        var history = GetCombatHistory(entity, opponent);
        return history?.Relationship ?? CombatRelationship.Neutral;
    }

    /// <summary>
    /// Checks if an entity should be afraid of an opponent.
    /// </summary>
    public bool IsAfraidOf(Entity entity, Entity opponent)
    {
        if (!entity.Has<CombatMemory>())
            return false;

        var memory = entity.Get<CombatMemory>();
        var opponentKey = GetEntityGuid(opponent);
        return memory.ShouldBeAfraid(opponentKey);
    }

    /// <summary>
    /// Checks if an entity is seeking revenge against an opponent.
    /// </summary>
    public bool IsSeekingRevenge(Entity entity, Entity opponent)
    {
        if (!entity.Has<CombatMemory>())
            return false;

        var memory = entity.Get<CombatMemory>();
        var opponentKey = GetEntityGuid(opponent);
        return memory.ShouldSeekRevenge(opponentKey);
    }

    /// <summary>
    /// Helper to get a consistent Guid for an entity.
    /// </summary>
    private Guid GetEntityGuid(Entity entity)
    {
        // Use entity ID as basis for Guid
        // In production, entities should have a proper Guid component
        return new Guid(entity.Id, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    }
}

/// <summary>
/// Tracks an active combat session.
/// </summary>
internal class ActiveCombat
{
    public required string CombatId { get; set; }
    public required Entity Attacker { get; set; }
    public required Entity Defender { get; set; }
    public required DateTime StartTime { get; set; }
    public required int AttackerDamageDealt { get; set; }
    public required int DefenderDamageDealt { get; set; }
    public required List<string> AttackerTactics { get; set; }
    public required List<string> DefenderTactics { get; set; }
    public required int TurnCount { get; set; }
}
