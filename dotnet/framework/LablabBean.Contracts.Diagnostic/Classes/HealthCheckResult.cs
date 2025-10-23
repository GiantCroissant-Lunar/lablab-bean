using System;
using System.Collections.Generic;
using System.Linq;

namespace LablabBean.Contracts.Diagnostic;

/// <summary>
/// Overall health check result across all providers.
/// </summary>
public class HealthCheckResult
{
    /// <summary>
    /// Overall health status.
    /// </summary>
    public SystemHealth OverallHealth { get; set; }

    /// <summary>
    /// Health check timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// Total health check duration.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Whether all checks passed.
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Individual provider health results.
    /// </summary>
    public List<ProviderHealthResult> ProviderResults { get; set; } = new();

    /// <summary>
    /// Summary of health issues.
    /// </summary>
    public List<string> HealthIssues { get; set; } = new();

    /// <summary>
    /// Aggregate health metrics.
    /// </summary>
    public Dictionary<string, object> AggregateMetrics { get; set; } = new();

    /// <summary>
    /// Health check context.
    /// </summary>
    public Dictionary<string, string> Context { get; set; } = new();

    /// <summary>
    /// Number of healthy providers.
    /// </summary>
    public int HealthyProviders => ProviderResults.Count(r => r.Health == SystemHealth.Healthy);

    /// <summary>
    /// Number of degraded providers.
    /// </summary>
    public int DegradedProviders => ProviderResults.Count(r => r.Health == SystemHealth.Degraded);

    /// <summary>
    /// Number of unhealthy providers.
    /// </summary>
    public int UnhealthyProviders => ProviderResults.Count(r => r.Health == SystemHealth.Degraded);

    /// <summary>
    /// Number of unknown status providers.
    /// </summary>
    public int UnknownProviders => ProviderResults.Count(r => r.Health == SystemHealth.Unknown);

    /// <summary>
    /// Total number of providers checked.
    /// </summary>
    public int TotalProviders => ProviderResults.Count;

    /// <summary>
    /// Calculate overall health from provider results.
    /// </summary>
    public void CalculateOverallHealth()
    {
        if (ProviderResults.Count == 0)
        {
            OverallHealth = SystemHealth.Unknown;
            IsSuccessful = false;
            return;
        }

        var unhealthyCount = UnhealthyProviders;
        var degradedCount = DegradedProviders;
        var healthyCount = HealthyProviders;

        if (unhealthyCount > 0)
        {
            OverallHealth = SystemHealth.Degraded;
            IsSuccessful = false;
        }
        else if (degradedCount > 0)
        {
            OverallHealth = SystemHealth.Degraded;
            IsSuccessful = true; // Degraded is still considered successful
        }
        else if (healthyCount > 0)
        {
            OverallHealth = SystemHealth.Healthy;
            IsSuccessful = true;
        }
        else
        {
            OverallHealth = SystemHealth.Unknown;
            IsSuccessful = false;
        }
    }

    /// <summary>
    /// Add a provider health result.
    /// </summary>
    public void AddProviderResult(ProviderHealthResult result)
    {
        ProviderResults.Add(result);

        if (!result.IsSuccessful && !string.IsNullOrEmpty(result.ErrorMessage))
        {
            HealthIssues.Add($"{result.ProviderName}: {result.ErrorMessage}");
        }
    }

    /// <summary>
    /// Get providers by health status.
    /// </summary>
    public IEnumerable<ProviderHealthResult> GetProvidersByHealth(SystemHealth health)
    {
        return ProviderResults.Where(r => r.Health == health);
    }

    /// <summary>
    /// Create a summary string of the health check.
    /// </summary>
    public string GetSummary()
    {
        return $"Overall Health: {OverallHealth} | " +
               $"Providers: {HealthyProviders} healthy, {DegradedProviders} degraded, {UnhealthyProviders} unhealthy | " +
               $"Duration: {Duration.TotalMilliseconds:F1}ms";
    }
}
