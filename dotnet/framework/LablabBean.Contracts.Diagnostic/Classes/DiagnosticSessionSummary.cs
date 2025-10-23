using System;
using System.Collections.Generic;

namespace LablabBean.Contracts.Diagnostic;

/// <summary>
/// Summary of a completed diagnostic session.
/// </summary>
public class DiagnosticSessionSummary
{
    /// <summary>
    /// Session ID.
    /// </summary>
    public string SessionId { get; set; } = "";

    /// <summary>
    /// Session name.
    /// </summary>
    public string SessionName { get; set; } = "";

    /// <summary>
    /// Session start time.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Session end time.
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Total session duration.
    /// </summary>
    public TimeSpan Duration => EndTime - StartTime;

    /// <summary>
    /// Session configuration used.
    /// </summary>
    public DiagnosticSessionConfig Configuration { get; set; } = new();

    /// <summary>
    /// Total number of data points collected.
    /// </summary>
    public int TotalDataPoints { get; set; }

    /// <summary>
    /// Number of events logged during the session.
    /// </summary>
    public int TotalEvents { get; set; }

    /// <summary>
    /// Number of alerts triggered during the session.
    /// </summary>
    public int TotalAlerts { get; set; }

    /// <summary>
    /// Providers that participated in the session.
    /// </summary>
    public List<string> ParticipatingProviders { get; set; } = new();

    /// <summary>
    /// Session performance summary.
    /// </summary>
    public SessionPerformanceSummary Performance { get; set; } = new();

    /// <summary>
    /// Session memory summary.
    /// </summary>
    public SessionMemorySummary Memory { get; set; } = new();

    /// <summary>
    /// System health during the session.
    /// </summary>
    public SessionHealthSummary Health { get; set; } = new();

    /// <summary>
    /// Errors encountered during the session.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Warnings encountered during the session.
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Session tags.
    /// </summary>
    public Dictionary<string, string> Tags { get; set; } = new();

    /// <summary>
    /// Session metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Export file path (if auto-export was enabled).
    /// </summary>
    public string? ExportPath { get; set; }

    /// <summary>
    /// Data collection statistics.
    /// </summary>
    public Dictionary<string, int> CollectionStats { get; set; } = new();

    /// <summary>
    /// Whether the session completed successfully.
    /// </summary>
    public bool IsSuccessful { get; set; } = true;

    /// <summary>
    /// Session completion reason.
    /// </summary>
    public string CompletionReason { get; set; } = "Manual stop";
}

/// <summary>
/// Performance summary for a session.
/// </summary>
public class SessionPerformanceSummary
{
    /// <summary>
    /// Average frame rate during the session.
    /// </summary>
    public float AverageFrameRate { get; set; }

    /// <summary>
    /// Minimum frame rate during the session.
    /// </summary>
    public float MinFrameRate { get; set; }

    /// <summary>
    /// Maximum frame rate during the session.
    /// </summary>
    public float MaxFrameRate { get; set; }

    /// <summary>
    /// Average frame time in milliseconds.
    /// </summary>
    public float AverageFrameTime { get; set; }

    /// <summary>
    /// Average CPU usage percentage.
    /// </summary>
    public float AverageCpuUsage { get; set; }

    /// <summary>
    /// Average GPU usage percentage.
    /// </summary>
    public float AverageGpuUsage { get; set; }

    /// <summary>
    /// Number of frame rate drops below threshold.
    /// </summary>
    public int FrameRateDrops { get; set; }

    /// <summary>
    /// Total number of draw calls.
    /// </summary>
    public long TotalDrawCalls { get; set; }

    /// <summary>
    /// Average draw calls per frame.
    /// </summary>
    public float AverageDrawCalls { get; set; }
}

/// <summary>
/// Memory summary for a session.
/// </summary>
public class SessionMemorySummary
{
    /// <summary>
    /// Average memory usage in bytes.
    /// </summary>
    public long AverageMemoryUsage { get; set; }

    /// <summary>
    /// Peak memory usage in bytes.
    /// </summary>
    public long PeakMemoryUsage { get; set; }

    /// <summary>
    /// Number of garbage collections during the session.
    /// </summary>
    public int GarbageCollections { get; set; }

    /// <summary>
    /// Total GC time in milliseconds.
    /// </summary>
    public float TotalGcTime { get; set; }

    /// <summary>
    /// Average graphics memory usage in bytes.
    /// </summary>
    public long AverageGraphicsMemoryUsage { get; set; }

    /// <summary>
    /// Peak graphics memory usage in bytes.
    /// </summary>
    public long PeakGraphicsMemoryUsage { get; set; }

    /// <summary>
    /// Memory snapshots taken during the session.
    /// </summary>
    public int MemorySnapshots { get; set; }
}

/// <summary>
/// Health summary for a session.
/// </summary>
public class SessionHealthSummary
{
    /// <summary>
    /// Overall session health status.
    /// </summary>
    public SystemHealth OverallHealth { get; set; }

    /// <summary>
    /// Percentage of time each health status was active.
    /// </summary>
    public Dictionary<SystemHealth, float> HealthDistribution { get; set; } = new();

    /// <summary>
    /// Number of health status changes during the session.
    /// </summary>
    public int HealthStatusChanges { get; set; }

    /// <summary>
    /// Time spent in each health status.
    /// </summary>
    public Dictionary<SystemHealth, TimeSpan> HealthDurations { get; set; } = new();

    /// <summary>
    /// Health check results during the session.
    /// </summary>
    public int TotalHealthChecks { get; set; }

    /// <summary>
    /// Failed health checks during the session.
    /// </summary>
    public int FailedHealthChecks { get; set; }
}
