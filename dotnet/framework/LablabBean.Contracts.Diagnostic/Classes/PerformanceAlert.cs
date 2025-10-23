using System;
using System.Collections.Generic;

namespace LablabBean.Contracts.Diagnostic;

/// <summary>
/// Represents a performance alert triggered by threshold violations.
/// </summary>
public class PerformanceAlert
{
    /// <summary>
    /// Alert ID.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Alert timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// Alert severity level.
    /// </summary>
    public DiagnosticLevel Severity { get; set; }

    /// <summary>
    /// Alert title.
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// Alert description.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Metric that triggered the alert.
    /// </summary>
    public string MetricName { get; set; } = "";

    /// <summary>
    /// Current metric value.
    /// </summary>
    public object? CurrentValue { get; set; }

    /// <summary>
    /// Threshold value that was exceeded.
    /// </summary>
    public object? ThresholdValue { get; set; }

    /// <summary>
    /// Provider that detected the alert.
    /// </summary>
    public string ProviderName { get; set; } = "";

    /// <summary>
    /// Alert category.
    /// </summary>
    public string Category { get; set; } = "";

    /// <summary>
    /// Associated performance metrics.
    /// </summary>
    public PerformanceMetrics? Metrics { get; set; }

    /// <summary>
    /// Additional context data.
    /// </summary>
    public Dictionary<string, object> Context { get; set; } = new();

    /// <summary>
    /// Suggested actions to resolve the alert.
    /// </summary>
    public List<string> SuggestedActions { get; set; } = new();

    /// <summary>
    /// Whether the alert is automatically resolved.
    /// </summary>
    public bool IsResolved { get; set; }

    /// <summary>
    /// When the alert was resolved.
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// Create a performance alert.
    /// </summary>
    public static PerformanceAlert Create(DiagnosticLevel severity, string title, string description, string metricName, object? currentValue = null, object? thresholdValue = null)
    {
        return new PerformanceAlert
        {
            Severity = severity,
            Title = title,
            Description = description,
            MetricName = metricName,
            CurrentValue = currentValue,
            ThresholdValue = thresholdValue
        };
    }

    /// <summary>
    /// Mark the alert as resolved.
    /// </summary>
    public void Resolve()
    {
        IsResolved = true;
        ResolvedAt = DateTime.Now;
    }
}
