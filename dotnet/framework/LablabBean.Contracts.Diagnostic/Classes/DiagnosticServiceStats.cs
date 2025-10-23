using System;
using System.Collections.Generic;

namespace LablabBean.Contracts.Diagnostic;

/// <summary>
/// Statistics about the diagnostic service itself.
/// </summary>
public class DiagnosticServiceStats
{
    /// <summary>
    /// Service start time.
    /// </summary>
    public DateTime ServiceStartTime { get; set; }

    /// <summary>
    /// Service uptime.
    /// </summary>
    public TimeSpan Uptime => DateTime.Now - ServiceStartTime;

    /// <summary>
    /// Whether data collection is currently active.
    /// </summary>
    public bool IsCollecting { get; set; }

    /// <summary>
    /// Current collection interval.
    /// </summary>
    public TimeSpan? CurrentCollectionInterval { get; set; }

    /// <summary>
    /// Total number of data collections performed.
    /// </summary>
    public long TotalDataCollections { get; set; }

    /// <summary>
    /// Total number of events logged.
    /// </summary>
    public long TotalEventsLogged { get; set; }

    /// <summary>
    /// Total number of alerts triggered.
    /// </summary>
    public long TotalAlertsTriggered { get; set; }

    /// <summary>
    /// Number of active diagnostic sessions.
    /// </summary>
    public int ActiveSessions { get; set; }

    /// <summary>
    /// Total number of sessions started.
    /// </summary>
    public long TotalSessionsStarted { get; set; }

    /// <summary>
    /// Total number of completed sessions.
    /// </summary>
    public long TotalSessionsCompleted { get; set; }

    /// <summary>
    /// Number of registered providers.
    /// </summary>
    public int RegisteredProviders { get; set; }

    /// <summary>
    /// Number of enabled providers.
    /// </summary>
    public int EnabledProviders { get; set; }

    /// <summary>
    /// Number of healthy providers.
    /// </summary>
    public int HealthyProviders { get; set; }

    /// <summary>
    /// Provider statistics by type.
    /// </summary>
    public Dictionary<ProviderType, ProviderTypeStats> ProviderStats { get; set; } = new();

    /// <summary>
    /// Total memory used by diagnostic data.
    /// </summary>
    public long DiagnosticDataMemoryUsage { get; set; }

    /// <summary>
    /// Number of diagnostic data records stored.
    /// </summary>
    public int StoredDataRecords { get; set; }

    /// <summary>
    /// Average data collection time in milliseconds.
    /// </summary>
    public double AverageCollectionTime { get; set; }

    /// <summary>
    /// Last collection time.
    /// </summary>
    public DateTime? LastCollectionTime { get; set; }

    /// <summary>
    /// Collection errors in the last hour.
    /// </summary>
    public int RecentCollectionErrors { get; set; }

    /// <summary>
    /// Last health check time.
    /// </summary>
    public DateTime? LastHealthCheckTime { get; set; }

    /// <summary>
    /// Overall service health.
    /// </summary>
    public SystemHealth ServiceHealth { get; set; }

    /// <summary>
    /// Service errors in the last hour.
    /// </summary>
    public int RecentServiceErrors { get; set; }

    /// <summary>
    /// Configuration changes since startup.
    /// </summary>
    public int ConfigurationChanges { get; set; }

    /// <summary>
    /// Data exports performed.
    /// </summary>
    public long DataExportsPerformed { get; set; }

    /// <summary>
    /// Total size of exported data in bytes.
    /// </summary>
    public long TotalExportedDataSize { get; set; }

    /// <summary>
    /// Performance metrics for the service itself.
    /// </summary>
    public ServicePerformanceMetrics Performance { get; set; } = new();

    /// <summary>
    /// Global tags currently set.
    /// </summary>
    public Dictionary<string, string> GlobalTags { get; set; } = new();

    /// <summary>
    /// User context information.
    /// </summary>
    public ServiceUserContext? UserContext { get; set; }
}

/// <summary>
/// Statistics for a specific provider type.
/// </summary>
public class ProviderTypeStats
{
    /// <summary>
    /// Provider type.
    /// </summary>
    public ProviderType Type { get; set; }

    /// <summary>
    /// Number of providers of this type.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Number of enabled providers of this type.
    /// </summary>
    public int EnabledCount { get; set; }

    /// <summary>
    /// Number of healthy providers of this type.
    /// </summary>
    public int HealthyCount { get; set; }

    /// <summary>
    /// Total data collections by providers of this type.
    /// </summary>
    public long TotalCollections { get; set; }

    /// <summary>
    /// Average collection time for this provider type.
    /// </summary>
    public double AverageCollectionTime { get; set; }

    /// <summary>
    /// Errors from providers of this type.
    /// </summary>
    public long TotalErrors { get; set; }
}

/// <summary>
/// Performance metrics for the diagnostic service itself.
/// </summary>
public class ServicePerformanceMetrics
{
    /// <summary>
    /// CPU usage by the diagnostic service.
    /// </summary>
    public float CpuUsage { get; set; }

    /// <summary>
    /// Memory usage by the diagnostic service in bytes.
    /// </summary>
    public long MemoryUsage { get; set; }

    /// <summary>
    /// Number of threads used by the service.
    /// </summary>
    public int ThreadCount { get; set; }

    /// <summary>
    /// Average response time for service operations.
    /// </summary>
    public double AverageResponseTime { get; set; }

    /// <summary>
    /// Operations per second.
    /// </summary>
    public double OperationsPerSecond { get; set; }

    /// <summary>
    /// Queue sizes for async operations.
    /// </summary>
    public Dictionary<string, int> QueueSizes { get; set; } = new();
}

/// <summary>
/// User context information for the diagnostic service.
/// </summary>
public class ServiceUserContext
{
    /// <summary>
    /// User ID.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// User properties.
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();

    /// <summary>
    /// When the user context was set.
    /// </summary>
    public DateTime SetAt { get; set; }
}
