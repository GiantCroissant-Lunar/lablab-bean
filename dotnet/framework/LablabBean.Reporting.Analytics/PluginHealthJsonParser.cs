using System.Text.Json;
using LablabBean.Reporting.Contracts.Models;
using Microsoft.Extensions.Logging;

namespace LablabBean.Reporting.Analytics;

/// <summary>
/// Parses plugin health data from JSON format.
/// Expected format: { "plugins": [...], "timestamp": "..." }
/// </summary>
public class PluginHealthJsonParser
{
    private readonly ILogger _logger;

    public PluginHealthJsonParser(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<PluginHealthData> ParsePluginHealthAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var data = new PluginHealthData();

        try
        {
            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            var healthSnapshot = JsonSerializer.Deserialize<PluginHealthSnapshot>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (healthSnapshot?.Plugins == null)
            {
                _logger.LogWarning("No plugins found in health snapshot at {FilePath}", filePath);
                return data;
            }

            // Convert to PluginStatus objects
            data.Plugins = healthSnapshot.Plugins.Select(p => new PluginStatus
            {
                Name = p.Name ?? "Unknown",
                Version = p.Version ?? "0.0.0",
                State = p.State ?? "Unknown",
                MemoryUsageMB = p.MemoryUsageMB,
                LoadDuration = p.LoadDurationMs > 0 ? TimeSpan.FromMilliseconds(p.LoadDurationMs) : TimeSpan.Zero,
                HealthStatusReason = p.HealthStatusReason,
                DegradedSince = p.DegradedSince,
                ErrorMessage = p.ErrorMessage,
                StackTrace = p.StackTrace
            }).ToList();

            // Calculate aggregates
            data.TotalPlugins = data.Plugins.Count;
            data.RunningPlugins = data.Plugins.Count(p => p.State == "Running");
            data.FailedPlugins = data.Plugins.Count(p => p.State == "Failed");
            data.DegradedPlugins = data.Plugins.Count(p => p.State == "Degraded");
            data.SuccessRate = data.TotalPlugins > 0 
                ? (decimal)data.RunningPlugins / data.TotalPlugins * 100 
                : 0;
            data.TotalMemoryUsageMB = data.Plugins.Sum(p => p.MemoryUsageMB);
            data.TotalLoadTime = TimeSpan.FromMilliseconds(data.Plugins.Sum(p => p.LoadDuration.TotalMilliseconds));

            _logger.LogInformation("Parsed plugin health: {Total} total, {Running} running, {Failed} failed, {Degraded} degraded",
                data.TotalPlugins, data.RunningPlugins, data.FailedPlugins, data.DegradedPlugins);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing plugin health from {FilePath}", filePath);
            throw;
        }

        return data;
    }

    private class PluginHealthSnapshot
    {
        public List<PluginDto>? Plugins { get; set; }
        public DateTime Timestamp { get; set; }
    }

    private class PluginDto
    {
        public string? Name { get; set; }
        public string? Version { get; set; }
        public string? State { get; set; }
        public long MemoryUsageMB { get; set; }
        public double LoadDurationMs { get; set; }
        public string? HealthStatusReason { get; set; }
        public DateTime? DegradedSince { get; set; }
        public string? ErrorMessage { get; set; }
        public string? StackTrace { get; set; }
    }
}
