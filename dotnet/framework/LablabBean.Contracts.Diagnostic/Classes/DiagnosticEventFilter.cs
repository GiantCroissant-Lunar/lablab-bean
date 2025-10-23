using System;
using System.Collections.Generic;

namespace LablabBean.Contracts.Diagnostic;

/// <summary>
/// Filter for diagnostic events.
/// </summary>
public class DiagnosticEventFilter
{
    /// <summary>
    /// Minimum event level to include.
    /// </summary>
    public DiagnosticLevel? MinLevel { get; set; }

    /// <summary>
    /// Maximum event level to include.
    /// </summary>
    public DiagnosticLevel? MaxLevel { get; set; }

    /// <summary>
    /// Specific levels to include.
    /// </summary>
    public HashSet<DiagnosticLevel>? IncludeLevels { get; set; }

    /// <summary>
    /// Categories to include (null for all).
    /// </summary>
    public HashSet<string>? IncludeCategories { get; set; }

    /// <summary>
    /// Categories to exclude.
    /// </summary>
    public HashSet<string>? ExcludeCategories { get; set; }

    /// <summary>
    /// Sources to include (null for all).
    /// </summary>
    public HashSet<string>? IncludeSources { get; set; }

    /// <summary>
    /// Sources to exclude.
    /// </summary>
    public HashSet<string>? ExcludeSources { get; set; }

    /// <summary>
    /// Message patterns to include (regex).
    /// </summary>
    public HashSet<string>? IncludeMessagePatterns { get; set; }

    /// <summary>
    /// Message patterns to exclude (regex).
    /// </summary>
    public HashSet<string>? ExcludeMessagePatterns { get; set; }

    /// <summary>
    /// Start time for event range.
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// End time for event range.
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Required tags (all must be present).
    /// </summary>
    public Dictionary<string, string>? RequiredTags { get; set; }

    /// <summary>
    /// Optional tags (at least one must be present).
    /// </summary>
    public Dictionary<string, string>? OptionalTags { get; set; }

    /// <summary>
    /// Thread IDs to include.
    /// </summary>
    public HashSet<int>? IncludeThreadIds { get; set; }

    /// <summary>
    /// Process IDs to include.
    /// </summary>
    public HashSet<int>? IncludeProcessIds { get; set; }

    /// <summary>
    /// Whether to include events with exceptions.
    /// </summary>
    public bool? HasException { get; set; }

    /// <summary>
    /// User IDs to include.
    /// </summary>
    public HashSet<string>? IncludeUserIds { get; set; }

    /// <summary>
    /// Session IDs to include.
    /// </summary>
    public HashSet<string>? IncludeSessionIds { get; set; }

    /// <summary>
    /// Maximum number of events to return.
    /// </summary>
    public int? Limit { get; set; }

    /// <summary>
    /// Create a filter for errors and warnings only.
    /// </summary>
    public static DiagnosticEventFilter ErrorsAndWarnings()
    {
        return new DiagnosticEventFilter
        {
            IncludeLevels = new HashSet<DiagnosticLevel> { DiagnosticLevel.Warning, DiagnosticLevel.Error, DiagnosticLevel.Critical }
        };
    }

    /// <summary>
    /// Create a filter for a specific category.
    /// </summary>
    public static DiagnosticEventFilter ForCategory(string category)
    {
        return new DiagnosticEventFilter
        {
            IncludeCategories = new HashSet<string> { category }
        };
    }

    /// <summary>
    /// Create a filter for a specific source.
    /// </summary>
    public static DiagnosticEventFilter ForSource(string source)
    {
        return new DiagnosticEventFilter
        {
            IncludeSources = new HashSet<string> { source }
        };
    }

    /// <summary>
    /// Create a filter for a specific time range.
    /// </summary>
    public static DiagnosticEventFilter ForTimeRange(DateTime startTime, DateTime endTime)
    {
        return new DiagnosticEventFilter
        {
            StartTime = startTime,
            EndTime = endTime
        };
    }

    /// <summary>
    /// Create a filter for events with exceptions.
    /// </summary>
    public static DiagnosticEventFilter WithExceptions()
    {
        return new DiagnosticEventFilter
        {
            HasException = true
        };
    }

    /// <summary>
    /// Create a filter for a specific user.
    /// </summary>
    public static DiagnosticEventFilter ForUser(string userId)
    {
        return new DiagnosticEventFilter
        {
            IncludeUserIds = new HashSet<string> { userId }
        };
    }

    /// <summary>
    /// Create a filter for a specific session.
    /// </summary>
    public static DiagnosticEventFilter ForSession(string sessionId)
    {
        return new DiagnosticEventFilter
        {
            IncludeSessionIds = new HashSet<string> { sessionId }
        };
    }

    /// <summary>
    /// Create a filter for recent events.
    /// </summary>
    public static DiagnosticEventFilter Recent(TimeSpan timeSpan)
    {
        return new DiagnosticEventFilter
        {
            StartTime = DateTime.Now - timeSpan
        };
    }

    /// <summary>
    /// Combine this filter with another using AND logic.
    /// </summary>
    public DiagnosticEventFilter And(DiagnosticEventFilter other)
    {
        var combined = new DiagnosticEventFilter();

        // Use the more restrictive level filters
        if (MinLevel.HasValue && other.MinLevel.HasValue)
            combined.MinLevel = (DiagnosticLevel)Math.Max((int)MinLevel.Value, (int)other.MinLevel.Value);
        else
            combined.MinLevel = MinLevel ?? other.MinLevel;

        if (MaxLevel.HasValue && other.MaxLevel.HasValue)
            combined.MaxLevel = (DiagnosticLevel)Math.Min((int)MaxLevel.Value, (int)other.MaxLevel.Value);
        else
            combined.MaxLevel = MaxLevel ?? other.MaxLevel;

        // Intersect include sets
        combined.IncludeCategories = IntersectSets(IncludeCategories, other.IncludeCategories);
        combined.IncludeSources = IntersectSets(IncludeSources, other.IncludeSources);

        // Union exclude sets
        combined.ExcludeCategories = UnionSets(ExcludeCategories, other.ExcludeCategories);
        combined.ExcludeSources = UnionSets(ExcludeSources, other.ExcludeSources);

        // Use the more restrictive time range
        combined.StartTime = StartTime.HasValue && other.StartTime.HasValue
            ? (StartTime > other.StartTime ? StartTime : other.StartTime)
            : StartTime ?? other.StartTime;

        combined.EndTime = EndTime.HasValue && other.EndTime.HasValue
            ? (EndTime < other.EndTime ? EndTime : other.EndTime)
            : EndTime ?? other.EndTime;

        return combined;
    }

    private static HashSet<T>? IntersectSets<T>(HashSet<T>? set1, HashSet<T>? set2)
    {
        if (set1 == null && set2 == null) return null;
        if (set1 == null) return set2;
        if (set2 == null) return set1;

        var intersection = new HashSet<T>(set1);
        intersection.IntersectWith(set2);
        return intersection.Count > 0 ? intersection : null;
    }

    private static HashSet<T>? UnionSets<T>(HashSet<T>? set1, HashSet<T>? set2)
    {
        if (set1 == null && set2 == null) return null;
        if (set1 == null) return set2;
        if (set2 == null) return set1;

        var union = new HashSet<T>(set1);
        union.UnionWith(set2);
        return union;
    }
}
