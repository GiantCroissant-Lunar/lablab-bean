using System;
using System.Collections.Generic;
using System.Linq;

namespace LablabBean.Plugins.NPC.Components;

/// <summary>
/// Tracks learned tactics, counter-strategies, and tactical analysis for adaptive AI.
/// Enables NPCs to learn from combat and adapt their strategies.
/// </summary>
public struct TacticalMemory
{
    /// <summary>
    /// Analysis of tactics this NPC has used (key: tactic name).
    /// </summary>
    public Dictionary<string, TacticAnalysis> LearnedTactics { get; set; }

    /// <summary>
    /// Counter-strategies learned against opponent tactics (key: opponent tactic).
    /// </summary>
    public Dictionary<string, TacticCounters> CounterStrategies { get; set; }

    /// <summary>
    /// List of tactics that have been successful recently.
    /// </summary>
    public List<string> RecentSuccesses { get; set; }

    /// <summary>
    /// List of tactics that have failed recently.
    /// </summary>
    public List<string> RecentFailures { get; set; }

    /// <summary>
    /// Preferred tactic based on success rates.
    /// </summary>
    public string? PreferredTactic { get; set; }

    /// <summary>
    /// Last time tactics were analyzed and updated.
    /// </summary>
    public DateTime LastAnalysis { get; set; }

    /// <summary>
    /// Creates a new TacticalMemory instance.
    /// </summary>
    public static TacticalMemory Create()
    {
        return new TacticalMemory
        {
            LearnedTactics = new Dictionary<string, TacticAnalysis>(),
            CounterStrategies = new Dictionary<string, TacticCounters>(),
            RecentSuccesses = new List<string>(),
            RecentFailures = new List<string>(),
            PreferredTactic = null,
            LastAnalysis = DateTime.MinValue
        };
    }

    /// <summary>
    /// Records usage of a tactic and its outcome.
    /// </summary>
    public void RecordTacticUsage(string tacticName, bool success, double damageDealt)
    {
        LearnedTactics ??= new Dictionary<string, TacticAnalysis>();

        if (!LearnedTactics.TryGetValue(tacticName, out var analysis))
        {
            analysis = new TacticAnalysis
            {
                TacticName = tacticName,
                TimesUsed = 0,
                SuccessCount = 0,
                TotalDamage = 0,
                FirstUsed = DateTime.UtcNow
            };
            LearnedTactics[tacticName] = analysis;
        }

        analysis.TimesUsed++;
        if (success)
        {
            analysis.SuccessCount++;
            AddRecentSuccess(tacticName);
        }
        else
        {
            AddRecentFailure(tacticName);
        }

        analysis.TotalDamage += damageDealt;
        analysis.LastUsed = DateTime.UtcNow;
        analysis.UpdateMetrics();

        LearnedTactics[tacticName] = analysis;
    }

    /// <summary>
    /// Records a counter-strategy learned from combat.
    /// </summary>
    public void RecordCounter(string opponentTactic, string counterTactic, bool successful)
    {
        CounterStrategies ??= new Dictionary<string, TacticCounters>();

        if (!CounterStrategies.TryGetValue(opponentTactic, out var counters))
        {
            counters = new TacticCounters
            {
                OpponentTactic = opponentTactic,
                EffectiveCounters = new List<string>(),
                CounterSuccessRates = new Dictionary<string, CounterRecord>()
            };
            CounterStrategies[opponentTactic] = counters;
        }

        if (!counters.CounterSuccessRates.TryGetValue(counterTactic, out var record))
        {
            record = new CounterRecord
            {
                TacticName = counterTactic,
                Attempts = 0,
                Successes = 0
            };
        }

        record.Attempts++;
        if (successful)
        {
            record.Successes++;
            if (!counters.EffectiveCounters.Contains(counterTactic))
            {
                counters.EffectiveCounters.Add(counterTactic);
            }
        }

        record.UpdateSuccessRate();
        counters.CounterSuccessRates[counterTactic] = record;
        counters.SortEffectiveCounters();

        CounterStrategies[opponentTactic] = counters;
    }

    /// <summary>
    /// Gets the best counter-tactic for an opponent's tactic.
    /// </summary>
    public string? GetBestCounter(string opponentTactic)
    {
        if (CounterStrategies == null || !CounterStrategies.TryGetValue(opponentTactic, out var counters))
            return null;

        if (counters.EffectiveCounters.Count == 0)
            return null;

        // Return the counter with the highest success rate
        var bestCounter = counters.CounterSuccessRates
            .Where(kvp => kvp.Value.SuccessRate > 0.5)
            .OrderByDescending(kvp => kvp.Value.SuccessRate)
            .ThenByDescending(kvp => kvp.Value.Attempts)
            .Select(kvp => kvp.Key)
            .FirstOrDefault();

        return bestCounter ?? counters.EffectiveCounters.FirstOrDefault();
    }

    /// <summary>
    /// Updates the preferred tactic based on success rates.
    /// </summary>
    public void UpdatePreferredTactic()
    {
        if (LearnedTactics == null || LearnedTactics.Count == 0)
        {
            PreferredTactic = null;
            return;
        }

        // Find tactic with best success rate (minimum 3 uses)
        var candidates = LearnedTactics.Values
            .Where(t => t.TimesUsed >= 3)
            .OrderByDescending(t => t.SuccessRate)
            .ThenByDescending(t => t.AverageDamage)
            .ToList();

        PreferredTactic = candidates.Count > 0 ? candidates[0].TacticName : null;
        LastAnalysis = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets tactics that should be avoided (low success rate).
    /// </summary>
    public List<string> GetTacticsToAvoid(double threshold = 0.3)
    {
        if (LearnedTactics == null)
            return new List<string>();

        return LearnedTactics.Values
            .Where(t => t.TimesUsed >= 3 && t.SuccessRate < threshold)
            .Select(t => t.TacticName)
            .ToList();
    }

    /// <summary>
    /// Gets the most successful tactics.
    /// </summary>
    public List<string> GetSuccessfulTactics(int count = 3)
    {
        if (LearnedTactics == null)
            return new List<string>();

        return LearnedTactics.Values
            .Where(t => t.TimesUsed >= 2)
            .OrderByDescending(t => t.SuccessRate)
            .ThenByDescending(t => t.AverageDamage)
            .Take(count)
            .Select(t => t.TacticName)
            .ToList();
    }

    private void AddRecentSuccess(string tactic)
    {
        RecentSuccesses ??= new List<string>();
        RecentSuccesses.Add(tactic);
        if (RecentSuccesses.Count > 10) // Keep last 10
        {
            RecentSuccesses.RemoveAt(0);
        }
    }

    private void AddRecentFailure(string tactic)
    {
        RecentFailures ??= new List<string>();
        RecentFailures.Add(tactic);
        if (RecentFailures.Count > 10) // Keep last 10
        {
            RecentFailures.RemoveAt(0);
        }
    }
}

/// <summary>
/// Analysis of a specific tactic's performance.
/// </summary>
public struct TacticAnalysis
{
    /// <summary>
    /// Name/ID of the tactic.
    /// </summary>
    public string TacticName { get; set; }

    /// <summary>
    /// Total times this tactic has been used.
    /// </summary>
    public int TimesUsed { get; set; }

    /// <summary>
    /// Number of times this tactic was successful.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Success rate (0.0 to 1.0).
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// Total damage dealt using this tactic.
    /// </summary>
    public double TotalDamage { get; set; }

    /// <summary>
    /// Average damage per use.
    /// </summary>
    public double AverageDamage { get; set; }

    /// <summary>
    /// When this tactic was first used.
    /// </summary>
    public DateTime FirstUsed { get; set; }

    /// <summary>
    /// When this tactic was last used.
    /// </summary>
    public DateTime LastUsed { get; set; }

    /// <summary>
    /// Updates calculated metrics.
    /// </summary>
    public void UpdateMetrics()
    {
        SuccessRate = TimesUsed > 0 ? (double)SuccessCount / TimesUsed : 0.0;
        AverageDamage = TimesUsed > 0 ? TotalDamage / TimesUsed : 0.0;
    }

    /// <summary>
    /// Determines if this tactic is reliable (enough uses with good success rate).
    /// </summary>
    public bool IsReliable(int minUses = 3, double minSuccessRate = 0.6)
    {
        return TimesUsed >= minUses && SuccessRate >= minSuccessRate;
    }
}

/// <summary>
/// Counter-strategies learned against an opponent's tactic.
/// </summary>
public struct TacticCounters
{
    /// <summary>
    /// The opponent tactic being countered.
    /// </summary>
    public string OpponentTactic { get; set; }

    /// <summary>
    /// List of effective counter tactics (sorted by success rate).
    /// </summary>
    public List<string> EffectiveCounters { get; set; }

    /// <summary>
    /// Detailed success rates for each counter tactic.
    /// </summary>
    public Dictionary<string, CounterRecord> CounterSuccessRates { get; set; }

    /// <summary>
    /// Sorts effective counters by success rate.
    /// </summary>
    public readonly void SortEffectiveCounters()
    {
        if (EffectiveCounters == null || CounterSuccessRates == null)
            return;

        var successRates = CounterSuccessRates; // Capture to avoid 'this' in lambda
        EffectiveCounters.Sort((a, b) =>
        {
            var rateA = successRates.TryGetValue(a, out var recordA) ? recordA.SuccessRate : 0;
            var rateB = successRates.TryGetValue(b, out var recordB) ? recordB.SuccessRate : 0;
            return rateB.CompareTo(rateA); // Descending order
        });
    }

    /// <summary>
    /// Gets the most effective counter.
    /// </summary>
    public readonly string? GetBestCounter()
    {
        if (EffectiveCounters == null || EffectiveCounters.Count == 0)
            return null;

        return EffectiveCounters.Count > 0 ? EffectiveCounters[0] : null;
    }
}

/// <summary>
/// Record of a counter-tactic's performance.
/// </summary>
public struct CounterRecord
{
    /// <summary>
    /// Name of the counter tactic.
    /// </summary>
    public string TacticName { get; set; }

    /// <summary>
    /// Number of times this counter was attempted.
    /// </summary>
    public int Attempts { get; set; }

    /// <summary>
    /// Number of successful counters.
    /// </summary>
    public int Successes { get; set; }

    /// <summary>
    /// Success rate (0.0 to 1.0).
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// Updates the success rate.
    /// </summary>
    public void UpdateSuccessRate()
    {
        SuccessRate = Attempts > 0 ? (double)Successes / Attempts : 0.0;
    }
}
