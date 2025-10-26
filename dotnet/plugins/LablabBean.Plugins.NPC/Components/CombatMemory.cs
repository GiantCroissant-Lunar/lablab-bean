using System;
using System.Collections.Generic;

namespace LablabBean.Plugins.NPC.Components;

/// <summary>
/// Tracks combat encounters, outcomes, and emotional states for NPCs.
/// Enables enemies to remember fights, adapt tactics, and develop combat relationships.
/// </summary>
public struct CombatMemory
{
    /// <summary>
    /// Combat history with each opponent (key: opponent entity ID).
    /// </summary>
    public Dictionary<Guid, CombatHistory> Encounters { get; set; }

    /// <summary>
    /// Timestamp of the last combat this NPC participated in.
    /// </summary>
    public DateTime LastCombatTime { get; set; }

    /// <summary>
    /// Total number of combat encounters.
    /// </summary>
    public int TotalCombats { get; set; }

    /// <summary>
    /// Total victories.
    /// </summary>
    public int Wins { get; set; }

    /// <summary>
    /// Total defeats.
    /// </summary>
    public int Losses { get; set; }

    /// <summary>
    /// Total draws/fled combats.
    /// </summary>
    public int Draws { get; set; }

    /// <summary>
    /// Current emotional state based on recent combat experiences.
    /// </summary>
    public CombatEmotionalState EmotionalState { get; set; }

    /// <summary>
    /// Total damage dealt across all combats.
    /// </summary>
    public int TotalDamageDealt { get; set; }

    /// <summary>
    /// Total damage received across all combats.
    /// </summary>
    public int TotalDamageReceived { get; set; }

    /// <summary>
    /// Creates a new CombatMemory instance with default values.
    /// </summary>
    public static CombatMemory Create()
    {
        return new CombatMemory
        {
            Encounters = new Dictionary<Guid, CombatHistory>(),
            LastCombatTime = DateTime.MinValue,
            TotalCombats = 0,
            Wins = 0,
            Losses = 0,
            Draws = 0,
            EmotionalState = CombatEmotionalState.Neutral,
            TotalDamageDealt = 0,
            TotalDamageReceived = 0
        };
    }

    /// <summary>
    /// Gets combat history with a specific opponent, creating new entry if needed.
    /// </summary>
    public CombatHistory GetOrCreateHistory(Guid opponentId)
    {
        Encounters ??= new Dictionary<Guid, CombatHistory>();

        if (!Encounters.TryGetValue(opponentId, out var history))
        {
            history = new CombatHistory
            {
                OpponentId = opponentId,
                Encounters = new List<CombatEncounter>(),
                Wins = 0,
                Losses = 0,
                Draws = 0,
                LastFight = DateTime.MinValue,
                Relationship = CombatRelationship.Neutral
            };
            Encounters[opponentId] = history;
        }

        return history;
    }

    /// <summary>
    /// Calculates win rate (0.0 to 1.0).
    /// </summary>
    public double GetWinRate()
    {
        if (TotalCombats == 0) return 0.0;
        return (double)Wins / TotalCombats;
    }

    /// <summary>
    /// Calculates average damage dealt per combat.
    /// </summary>
    public double GetAverageDamageDealt()
    {
        if (TotalCombats == 0) return 0.0;
        return (double)TotalDamageDealt / TotalCombats;
    }

    /// <summary>
    /// Calculates average damage received per combat.
    /// </summary>
    public double GetAverageDamageReceived()
    {
        if (TotalCombats == 0) return 0.0;
        return (double)TotalDamageReceived / TotalCombats;
    }

    /// <summary>
    /// Determines if NPC should be afraid based on recent combat performance.
    /// </summary>
    public bool ShouldBeAfraid(Guid opponentId, int lossThreshold = 3)
    {
        if (!Encounters.TryGetValue(opponentId, out var history))
            return false;

        // Afraid if lost multiple times with no wins
        return history.Losses >= lossThreshold && history.Wins == 0;
    }

    /// <summary>
    /// Determines if NPC should seek revenge against an opponent.
    /// </summary>
    public bool ShouldSeekRevenge(Guid opponentId)
    {
        if (!Encounters.TryGetValue(opponentId, out var history))
            return false;

        // Seek revenge if recently lost but has won before (knows they can win)
        var timeSinceLastFight = DateTime.UtcNow - history.LastFight;
        return history.Losses > history.Wins
               && history.Wins > 0
               && timeSinceLastFight < TimeSpan.FromDays(7);
    }
}

/// <summary>
/// Combat history with a specific opponent.
/// </summary>
public class CombatHistory
{
    /// <summary>
    /// ID of the opponent.
    /// </summary>
    public Guid OpponentId { get; set; }

    /// <summary>
    /// List of all combat encounters with this opponent.
    /// </summary>
    public List<CombatEncounter> Encounters { get; set; } = new();

    /// <summary>
    /// Number of wins against this opponent.
    /// </summary>
    public int Wins { get; set; }

    /// <summary>
    /// Number of losses against this opponent.
    /// </summary>
    public int Losses { get; set; }

    /// <summary>
    /// Number of draws/fled combats.
    /// </summary>
    public int Draws { get; set; }

    /// <summary>
    /// Timestamp of the last fight with this opponent.
    /// </summary>
    public DateTime LastFight { get; set; }

    /// <summary>
    /// Relationship type based on combat history.
    /// </summary>
    public CombatRelationship Relationship { get; set; }

    /// <summary>
    /// Gets the most recent encounter.
    /// </summary>
    public CombatEncounter? GetLastEncounter()
    {
        return Encounters.Count > 0 ? Encounters[^1] : null;
    }

    /// <summary>
    /// Calculates win rate against this opponent.
    /// </summary>
    public double GetWinRate()
    {
        int totalFights = Wins + Losses + Draws;
        if (totalFights == 0) return 0.0;
        return (double)Wins / totalFights;
    }

    /// <summary>
    /// Updates combat relationship based on win/loss ratio.
    /// </summary>
    public void UpdateRelationship()
    {
        double winRate = GetWinRate();
        int totalFights = Wins + Losses + Draws;

        if (totalFights < 2)
        {
            Relationship = CombatRelationship.Neutral;
        }
        else if (Losses >= 3 && Wins == 0)
        {
            Relationship = CombatRelationship.Afraid;
        }
        else if (Losses >= 5)
        {
            Relationship = CombatRelationship.Nemesis;
        }
        else if (Losses > Wins && Wins > 0)
        {
            Relationship = CombatRelationship.Hunter; // Lost more but has won - seeking revenge
        }
        else if (Math.Abs(winRate - 0.5) < 0.2 && totalFights >= 3)
        {
            Relationship = CombatRelationship.Rival; // Evenly matched
        }
        else
        {
            Relationship = CombatRelationship.Neutral;
        }
    }
}

/// <summary>
/// Details of a single combat encounter.
/// </summary>
public class CombatEncounter
{
    /// <summary>
    /// When the combat occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Outcome of the combat from this NPC's perspective.
    /// </summary>
    public CombatOutcome Outcome { get; set; }

    /// <summary>
    /// Total damage this NPC dealt in the encounter.
    /// </summary>
    public int DamageDealt { get; set; }

    /// <summary>
    /// Total damage this NPC received in the encounter.
    /// </summary>
    public int DamageReceived { get; set; }

    /// <summary>
    /// Tactics/attacks this NPC used.
    /// </summary>
    public List<string> TacticsUsed { get; set; } = new();

    /// <summary>
    /// Tactics/attacks the opponent used.
    /// </summary>
    public List<string> OpponentTactics { get; set; } = new();

    /// <summary>
    /// How long the combat lasted.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Number of turns/rounds to complete.
    /// </summary>
    public int TurnsToComplete { get; set; }

    /// <summary>
    /// Health percentage when combat ended.
    /// </summary>
    public double EndingHealthPercent { get; set; }

    /// <summary>
    /// Additional context or notes about the encounter.
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Combat outcome from NPC's perspective.
/// </summary>
public enum CombatOutcome
{
    /// <summary>
    /// NPC won the fight.
    /// </summary>
    Win,

    /// <summary>
    /// NPC lost the fight.
    /// </summary>
    Loss,

    /// <summary>
    /// NPC fled from combat.
    /// </summary>
    Fled,

    /// <summary>
    /// Combat ended in a draw (timeout, both fled, etc.).
    /// </summary>
    Draw
}

/// <summary>
/// Emotional state based on combat experiences.
/// Affects behavior, dialogue, and tactics.
/// </summary>
public enum CombatEmotionalState
{
    /// <summary>
    /// Default state, no strong feelings.
    /// </summary>
    Neutral,

    /// <summary>
    /// NPC is confident due to recent victories.
    /// More aggressive, takes risks.
    /// </summary>
    Confident,

    /// <summary>
    /// NPC is cautious due to even odds or unknown opponent.
    /// Defensive, calculated approach.
    /// </summary>
    Cautious,

    /// <summary>
    /// NPC fears opponent due to previous defeats.
    /// Likely to flee, very defensive.
    /// </summary>
    Afraid,

    /// <summary>
    /// NPC seeks revenge for previous losses.
    /// Highly aggressive, reckless.
    /// </summary>
    Vengeful,

    /// <summary>
    /// NPC is desperate (low health, cornered, multiple defeats).
    /// Unpredictable, may use risky tactics or surrender.
    /// </summary>
    Desperate
}

/// <summary>
/// Combat-based relationship types.
/// </summary>
public enum CombatRelationship
{
    /// <summary>
    /// No established combat relationship.
    /// </summary>
    Neutral,

    /// <summary>
    /// Evenly matched opponents, mutual respect.
    /// </summary>
    Rival,

    /// <summary>
    /// Bitter enemy, multiple defeats, intense hatred.
    /// </summary>
    Nemesis,

    /// <summary>
    /// NPC is afraid of opponent due to domination.
    /// </summary>
    Afraid,

    /// <summary>
    /// NPC is hunting opponent for revenge.
    /// </summary>
    Hunter
}
