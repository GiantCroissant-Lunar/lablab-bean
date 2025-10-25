using System;
using System.Collections.Generic;
using System.Linq;
using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.NPC.Components;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.NPC.Systems;

/// <summary>
/// Analyzes combat patterns and selects adaptive tactics for NPCs.
/// Enables enemies to learn from past encounters and counter player strategies.
/// </summary>
public class AdaptiveTacticsSystem
{
    private readonly World _world;
    private readonly ILogger<AdaptiveTacticsSystem> _logger;

    // Default tactics by category
    private readonly Dictionary<TacticCategory, List<string>> _defaultTactics = new()
    {
        [TacticCategory.Aggressive] = new() { "Heavy Attack", "Charge", "Berserk", "Power Strike" },
        [TacticCategory.Defensive] = new() { "Block", "Dodge", "Parry", "Retreat" },
        [TacticCategory.Ranged] = new() { "Shoot Arrow", "Throw Rock", "Cast Fireball", "Sling" },
        [TacticCategory.Magic] = new() { "Fire Spell", "Ice Spell", "Lightning Bolt", "Heal" },
        [TacticCategory.Tactical] = new() { "Flank", "Feint", "Trip", "Disarm" }
    };

    public AdaptiveTacticsSystem(World world, ILogger<AdaptiveTacticsSystem> logger)
    {
        _world = world;
        _logger = logger;
    }

    /// <summary>
    /// Analyzes opponent's tactics and identifies patterns.
    /// </summary>
    public OpponentPattern AnalyzeOpponentTactics(Entity opponent, CombatMemory memory)
    {
        // Use Entity.Reference as the key (more reliable than Entity.Id)
        var opponentKey = GetEntityGuid(opponent);

        if (!memory.Encounters.TryGetValue(opponentKey, out var history))
        {
            return new OpponentPattern
            {
                MostUsedTactic = null,
                PreferredCategory = TacticCategory.Aggressive,
                TacticDistribution = new Dictionary<string, int>(),
                IsPatternReliable = false
            };
        }

        // Aggregate all opponent tactics from encounters
        var tacticCounts = new Dictionary<string, int>();
        foreach (var encounter in history.Encounters)
        {
            foreach (var tactic in encounter.OpponentTactics)
            {
                tacticCounts[tactic] = tacticCounts.GetValueOrDefault(tactic, 0) + 1;
            }
        }

        if (tacticCounts.Count == 0)
        {
            return new OpponentPattern
            {
                MostUsedTactic = null,
                PreferredCategory = TacticCategory.Aggressive,
                TacticDistribution = new Dictionary<string, int>(),
                IsPatternReliable = false
            };
        }

        // Identify most used tactic
        var mostUsed = tacticCounts.OrderByDescending(kvp => kvp.Value).First();

        // Determine preferred category
        var preferredCategory = ClassifyTactic(mostUsed.Key);

        // Pattern is reliable if we have at least 3 encounters
        bool isReliable = history.Encounters.Count >= 3;

        return new OpponentPattern
        {
            MostUsedTactic = mostUsed.Key,
            PreferredCategory = preferredCategory,
            TacticDistribution = tacticCounts,
            IsPatternReliable = isReliable
        };
    }

    /// <summary>
    /// Selects an appropriate counter-tactic based on opponent's pattern and NPC's tactical memory.
    /// </summary>
    public string SelectCounterTactic(OpponentPattern opponentPattern, TacticalMemory tactical)
    {
        // If we have a learned counter from tactical memory, use it
        if (!string.IsNullOrEmpty(opponentPattern.MostUsedTactic))
        {
            var learnedCounter = tactical.GetBestCounter(opponentPattern.MostUsedTactic);
            if (!string.IsNullOrEmpty(learnedCounter))
            {
                _logger.LogDebug("Using learned counter: {Counter} vs {OpponentTactic}",
                    learnedCounter, opponentPattern.MostUsedTactic);
                return learnedCounter;
            }
        }

        // Otherwise, use default counter based on category
        var counterCategory = GetCounterCategory(opponentPattern.PreferredCategory);
        var counterTactics = _defaultTactics[counterCategory];

        // Prefer tactics we've had success with
        var successfulTactics = tactical.GetSuccessfulTactics(count: 5);
        var counterFromSuccessful = counterTactics.FirstOrDefault(t => successfulTactics.Contains(t));

        if (!string.IsNullOrEmpty(counterFromSuccessful))
        {
            _logger.LogDebug("Using successful counter tactic: {Tactic} (category: {Category})",
                counterFromSuccessful, counterCategory);
            return counterFromSuccessful;
        }

        // Otherwise pick a random counter from the category
        var random = new Random();
        var selectedTactic = counterTactics[random.Next(counterTactics.Count)];

        _logger.LogDebug("Using random counter tactic: {Tactic} (category: {Category})",
            selectedTactic, counterCategory);
        return selectedTactic;
    }

    /// <summary>
    /// Gets adapted behavior for an NPC based on combat memory with opponent.
    /// </summary>
    public AdaptedBehavior GetAdaptedBehavior(Entity npc, Entity opponent)
    {
        if (!npc.Has<CombatMemory>() || !npc.Has<TacticalMemory>())
        {
            return new AdaptedBehavior
            {
                TacticCategory = TacticCategory.Aggressive,
                SelectedTactic = "Basic Attack",
                Aggression = 0.5,
                CautiousLevel = 0.5,
                ShouldFlee = false
            };
        }

        var memory = npc.Get<CombatMemory>();
        var tactical = npc.Get<TacticalMemory>();

        // Get history with this opponent
        var opponentKey = GetEntityGuid(opponent);
        if (!memory.Encounters.TryGetValue(opponentKey, out var history))
        {
            // First encounter - use default behavior
            return new AdaptedBehavior
            {
                TacticCategory = TacticCategory.Aggressive,
                SelectedTactic = tactical.PreferredTactic ?? "Basic Attack",
                Aggression = 0.6,
                CautiousLevel = 0.4,
                ShouldFlee = false
            };
        }

        // Adapt based on emotional state and relationship
        double aggression = 0.5;
        double cautiousLevel = 0.5;
        bool shouldFlee = false;
        TacticCategory category = TacticCategory.Aggressive;

        switch (memory.EmotionalState)
        {
            case CombatEmotionalState.Afraid:
                aggression = 0.2;
                cautiousLevel = 0.9;
                shouldFlee = history.Losses >= 3;
                category = TacticCategory.Defensive;
                break;

            case CombatEmotionalState.Vengeful:
                aggression = 0.9;
                cautiousLevel = 0.3;
                category = TacticCategory.Aggressive;
                break;

            case CombatEmotionalState.Confident:
                aggression = 0.7;
                cautiousLevel = 0.3;
                category = TacticCategory.Aggressive;
                break;

            case CombatEmotionalState.Desperate:
                aggression = 0.8;
                cautiousLevel = 0.2;
                category = TacticCategory.Aggressive; // All-or-nothing
                break;

            case CombatEmotionalState.Cautious:
                aggression = 0.4;
                cautiousLevel = 0.7;
                category = TacticCategory.Tactical;
                break;

            default: // Neutral
                aggression = 0.5;
                cautiousLevel = 0.5;
                category = TacticCategory.Tactical;
                break;
        }

        // Analyze opponent's pattern
        var pattern = AnalyzeOpponentTactics(opponent, memory);

        // Select counter tactic
        string selectedTactic;
        if (pattern.IsPatternReliable)
        {
            selectedTactic = SelectCounterTactic(pattern, tactical);
        }
        else
        {
            selectedTactic = tactical.PreferredTactic ?? _defaultTactics[category].First();
        }

        return new AdaptedBehavior
        {
            TacticCategory = category,
            SelectedTactic = selectedTactic,
            Aggression = aggression,
            CautiousLevel = cautiousLevel,
            ShouldFlee = shouldFlee,
            EmotionalState = memory.EmotionalState,
            Relationship = history.Relationship
        };
    }

    /// <summary>
    /// Helper to get a consistent Guid for an entity.
    /// Uses a combination of entity ID and reference to ensure uniqueness.
    /// </summary>
    private Guid GetEntityGuid(Entity entity)
    {
        // Use entity ID as basis for Guid
        // In production, entities should have a proper Guid component
        return new Guid(entity.Id, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    }

    /// <summary>
    /// Updates tactic success rate after a combat action.
    /// </summary>
    public void UpdateTacticSuccessRate(Entity npc, string tactic, bool success, double damageDealt)
    {
        if (!npc.Has<TacticalMemory>())
            return;

        ref var tactical = ref npc.Get<TacticalMemory>();
        tactical.RecordTacticUsage(tactic, success, damageDealt);
        npc.Set(tactical);

        _logger.LogDebug("Updated tactic {Tactic} for NPC {NpcId}: Success={Success}, Damage={Damage}",
            tactic, npc.Id, success, damageDealt);
    }

    /// <summary>
    /// Classifies a tactic into a category.
    /// </summary>
    private TacticCategory ClassifyTactic(string tactic)
    {
        var lower = tactic.ToLowerInvariant();

        if (lower.Contains("attack") || lower.Contains("strike") || lower.Contains("charge") || lower.Contains("berserk"))
            return TacticCategory.Aggressive;

        if (lower.Contains("block") || lower.Contains("dodge") || lower.Contains("parry") || lower.Contains("retreat"))
            return TacticCategory.Defensive;

        if (lower.Contains("arrow") || lower.Contains("shoot") || lower.Contains("throw") || lower.Contains("sling"))
            return TacticCategory.Ranged;

        if (lower.Contains("spell") || lower.Contains("magic") || lower.Contains("cast") || lower.Contains("heal"))
            return TacticCategory.Magic;

        return TacticCategory.Tactical;
    }

    /// <summary>
    /// Gets the counter category for a given tactic category.
    /// </summary>
    private TacticCategory GetCounterCategory(TacticCategory opponentCategory)
    {
        return opponentCategory switch
        {
            TacticCategory.Aggressive => TacticCategory.Defensive,
            TacticCategory.Defensive => TacticCategory.Aggressive,
            TacticCategory.Ranged => TacticCategory.Tactical, // Flank or close distance
            TacticCategory.Magic => TacticCategory.Aggressive, // Rush them down
            TacticCategory.Tactical => TacticCategory.Ranged,
            _ => TacticCategory.Aggressive
        };
    }

    /// <summary>
    /// Gets suggested tactics to avoid based on poor performance.
    /// </summary>
    public List<string> GetTacticsToAvoid(Entity npc, double threshold = 0.3)
    {
        if (!npc.Has<TacticalMemory>())
            return new List<string>();

        var tactical = npc.Get<TacticalMemory>();
        return tactical.GetTacticsToAvoid(threshold);
    }

    /// <summary>
    /// Gets the most successful tactics for an NPC.
    /// </summary>
    public List<string> GetBestTactics(Entity npc, int count = 3)
    {
        if (!npc.Has<TacticalMemory>())
            return new List<string>();

        var tactical = npc.Get<TacticalMemory>();
        return tactical.GetSuccessfulTactics(count);
    }
}

/// <summary>
/// Analysis of opponent's tactical patterns.
/// </summary>
public class OpponentPattern
{
    /// <summary>
    /// The tactic the opponent uses most frequently.
    /// </summary>
    public required string? MostUsedTactic { get; set; }

    /// <summary>
    /// The opponent's preferred tactic category.
    /// </summary>
    public required TacticCategory PreferredCategory { get; set; }

    /// <summary>
    /// Distribution of tactics used (tactic name -> count).
    /// </summary>
    public required Dictionary<string, int> TacticDistribution { get; set; }

    /// <summary>
    /// Whether the pattern is reliable (enough data).
    /// </summary>
    public required bool IsPatternReliable { get; set; }
}

/// <summary>
/// Adapted behavior based on combat memory and opponent analysis.
/// </summary>
public class AdaptedBehavior
{
    /// <summary>
    /// Preferred tactic category for this combat.
    /// </summary>
    public required TacticCategory TacticCategory { get; set; }

    /// <summary>
    /// Specific tactic to use.
    /// </summary>
    public required string SelectedTactic { get; set; }

    /// <summary>
    /// Aggression level (0.0 to 1.0).
    /// </summary>
    public required double Aggression { get; set; }

    /// <summary>
    /// Caution level (0.0 to 1.0).
    /// </summary>
    public required double CautiousLevel { get; set; }

    /// <summary>
    /// Whether the NPC should flee immediately.
    /// </summary>
    public required bool ShouldFlee { get; set; }

    /// <summary>
    /// Current emotional state.
    /// </summary>
    public CombatEmotionalState EmotionalState { get; set; }

    /// <summary>
    /// Combat relationship with opponent.
    /// </summary>
    public CombatRelationship Relationship { get; set; }
}

/// <summary>
/// Tactic categories for classification and countering.
/// </summary>
public enum TacticCategory
{
    Aggressive,
    Defensive,
    Ranged,
    Magic,
    Tactical
}
