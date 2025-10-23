using System;
using System.Collections.Generic;

namespace LablabBean.Contracts.Performance;

public class PerformanceStatistics
{
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan CollectionPeriod { get; set; }
    public Dictionary<string, MetricStatistics> Metrics { get; set; } = new();
    public Dictionary<string, long> Counters { get; set; } = new();
    public Dictionary<string, double> Gauges { get; set; } = new();
    public long TotalActivities { get; set; }
    public long FailedActivities { get; set; }
    public long TotalExceptions { get; set; }
    public MemoryStatistics Memory { get; set; } = new();
    public Dictionary<string, object> SystemMetrics { get; set; } = new();
}

public class MetricStatistics
{
    public string MetricName { get; set; } = string.Empty;
    public long Count { get; set; }
    public TimeSpan MinDuration { get; set; }
    public TimeSpan MaxDuration { get; set; }
    public TimeSpan AverageDuration { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public double StandardDeviation { get; set; }
    public TimeSpan[] Percentiles { get; set; } = Array.Empty<TimeSpan>();
}

public class MemoryStatistics
{
    public long TotalMemory { get; set; }
    public long WorkingSet { get; set; }
    public long PrivateMemory { get; set; }
    public int Gen0Collections { get; set; }
    public int Gen1Collections { get; set; }
    public int Gen2Collections { get; set; }
    public long AllocatedBytes { get; set; }
}

public class PerformanceHealthStatus
{
    public bool IsHealthy { get; set; }
    public DateTime LastHealthCheck { get; set; }
    public string[] Issues { get; set; } = Array.Empty<string>();
    public Dictionary<string, object> HealthMetrics { get; set; } = new();
    public double DataCollectionRate { get; set; }
    public long BufferUsage { get; set; }
    public long MaxBufferSize { get; set; }
    public TimeSpan AverageProcessingTime { get; set; }
}

public class PerformanceRecommendation
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PerformanceRecommendationSeverity Severity { get; set; }
    public string Category { get; set; } = string.Empty;
    public Dictionary<string, object> Evidence { get; set; } = new();
    public string[] SuggestedActions { get; set; } = Array.Empty<string>();
    public TimeSpan EstimatedImpact { get; set; }
}
