using System;
using System.Collections.Generic;

namespace LablabBean.Contracts.Diagnostic;

/// <summary>
/// Result of a provider health check.
/// </summary>
public class ProviderHealthResult
{
    /// <summary>
    /// Provider name.
    /// </summary>
    public string ProviderName { get; set; } = "";

    /// <summary>
    /// Health status.
    /// </summary>
    public SystemHealth Health { get; set; }

    /// <summary>
    /// Health check timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// Health check duration.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Health check success.
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Error message (if any).
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Health check details.
    /// </summary>
    public List<HealthCheckDetail> Details { get; set; } = new();

    /// <summary>
    /// Health metrics.
    /// </summary>
    public Dictionary<string, object> Metrics { get; set; } = new();

    /// <summary>
    /// Additional context.
    /// </summary>
    public Dictionary<string, string> Context { get; set; } = new();

    /// <summary>
    /// Create a successful health result.
    /// </summary>
    public static ProviderHealthResult Success(string providerName, SystemHealth health = SystemHealth.Healthy)
    {
        return new ProviderHealthResult
        {
            ProviderName = providerName,
            Health = health,
            IsSuccessful = true
        };
    }

    /// <summary>
    /// Create a failed health result.
    /// </summary>
    public static ProviderHealthResult Failure(string providerName, string errorMessage, SystemHealth health = SystemHealth.Degraded)
    {
        return new ProviderHealthResult
        {
            ProviderName = providerName,
            Health = health,
            IsSuccessful = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Individual health check detail.
/// </summary>
public class HealthCheckDetail
{
    /// <summary>
    /// Check name.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Check result.
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Check message.
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// Check severity.
    /// </summary>
    public DiagnosticLevel Severity { get; set; } = DiagnosticLevel.Information;

    /// <summary>
    /// Check value (if applicable).
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// Expected value (if applicable).
    /// </summary>
    public object? ExpectedValue { get; set; }
}
