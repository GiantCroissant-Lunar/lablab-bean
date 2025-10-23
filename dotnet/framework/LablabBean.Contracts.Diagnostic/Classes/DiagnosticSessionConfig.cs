using System;
using System.Collections.Generic;

namespace LablabBean.Contracts.Diagnostic;

/// <summary>
/// Configuration for a diagnostic session.
/// </summary>
public class DiagnosticSessionConfig
{
    /// <summary>
    /// Data collection interval.
    /// </summary>
    public TimeSpan CollectionInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Maximum session duration.
    /// </summary>
    public TimeSpan? MaxDuration { get; set; }

    /// <summary>
    /// Maximum number of data points to collect.
    /// </summary>
    public int? MaxDataPoints { get; set; }

    /// <summary>
    /// Providers to include in the session (null for all).
    /// </summary>
    public string[]? IncludeProviders { get; set; }

    /// <summary>
    /// Providers to exclude from the session.
    /// </summary>
    public string[]? ExcludeProviders { get; set; }

    /// <summary>
    /// Performance thresholds for alerts during the session.
    /// </summary>
    public PerformanceThresholds? AlertThresholds { get; set; }

    /// <summary>
    /// Whether to enable automatic data export.
    /// </summary>
    public bool AutoExport { get; set; }

    /// <summary>
    /// Export format for automatic export.
    /// </summary>
    public DiagnosticExportFormat ExportFormat { get; set; } = DiagnosticExportFormat.Json;

    /// <summary>
    /// Export file path for automatic export.
    /// </summary>
    public string? ExportPath { get; set; }

    /// <summary>
    /// Tags to apply to all session data.
    /// </summary>
    public Dictionary<string, string> SessionTags { get; set; } = new();

    /// <summary>
    /// Additional session metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Whether to collect detailed performance metrics.
    /// </summary>
    public bool CollectDetailedMetrics { get; set; } = true;

    /// <summary>
    /// Whether to collect memory snapshots.
    /// </summary>
    public bool CollectMemorySnapshots { get; set; }

    /// <summary>
    /// Memory snapshot interval.
    /// </summary>
    public TimeSpan MemorySnapshotInterval { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Whether to collect system information.
    /// </summary>
    public bool CollectSystemInfo { get; set; } = true;

    /// <summary>
    /// Whether to enable continuous profiling.
    /// </summary>
    public bool EnableProfiling { get; set; }

    /// <summary>
    /// Create default session configuration.
    /// </summary>
    public static DiagnosticSessionConfig CreateDefault()
    {
        return new DiagnosticSessionConfig();
    }

    /// <summary>
    /// Create lightweight session configuration for minimal overhead.
    /// </summary>
    public static DiagnosticSessionConfig CreateLightweight()
    {
        return new DiagnosticSessionConfig
        {
            CollectionInterval = TimeSpan.FromSeconds(5),
            CollectDetailedMetrics = false,
            CollectMemorySnapshots = false,
            CollectSystemInfo = false,
            EnableProfiling = false
        };
    }

    /// <summary>
    /// Create comprehensive session configuration for detailed analysis.
    /// </summary>
    public static DiagnosticSessionConfig CreateComprehensive()
    {
        return new DiagnosticSessionConfig
        {
            CollectionInterval = TimeSpan.FromMilliseconds(500),
            CollectDetailedMetrics = true,
            CollectMemorySnapshots = true,
            MemorySnapshotInterval = TimeSpan.FromSeconds(30),
            CollectSystemInfo = true,
            EnableProfiling = true,
            AutoExport = true,
            ExportFormat = DiagnosticExportFormat.Json
        };
    }
}
