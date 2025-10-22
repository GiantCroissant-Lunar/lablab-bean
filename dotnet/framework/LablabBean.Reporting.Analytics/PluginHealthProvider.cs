using LablabBean.Reporting.Abstractions.Attributes;
using LablabBean.Reporting.Abstractions.Contracts;
using LablabBean.Reporting.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace LablabBean.Reporting.Analytics;

/// <summary>
/// Provides plugin system health data including loaded plugins, states, memory usage, and load times.
/// </summary>
[ReportProvider("PluginHealth", "Diagnostics", priority: 0)]
public class PluginHealthProvider : IReportProvider
{
    private readonly ILogger<PluginHealthProvider> _logger;

    public PluginHealthProvider(ILogger<PluginHealthProvider> logger)
    {
        _logger = logger;
    }

    public async Task<object> GetReportDataAsync(ReportRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Collecting plugin health metrics from {DataPath}", request.DataPath ?? "plugin system");

        var data = new PluginHealthData();

        try
        {
            if (!string.IsNullOrWhiteSpace(request.DataPath) && File.Exists(request.DataPath))
            {
                // Parse real plugin health data from JSON file
                var parser = new PluginHealthJsonParser(_logger);
                var healthData = await parser.ParsePluginHealthAsync(request.DataPath, cancellationToken);
                
                data.TotalPlugins = healthData.TotalPlugins;
                data.RunningPlugins = healthData.RunningPlugins;
                data.FailedPlugins = healthData.FailedPlugins;
                data.DegradedPlugins = healthData.DegradedPlugins;
                data.SuccessRate = healthData.SuccessRate;
                data.Plugins = healthData.Plugins;
                data.TotalMemoryUsageMB = healthData.TotalMemoryUsageMB;
                data.TotalLoadTime = healthData.TotalLoadTime;
                
                _logger.LogInformation("Loaded {PluginCount} plugins, {RunningCount} running, {FailedCount} failed", 
                    data.TotalPlugins, data.RunningPlugins, data.FailedPlugins);
            }
            else
            {
                // Generate sample data for demonstration
                _logger.LogWarning("No plugin health data found at {DataPath}, generating sample data", request.DataPath);
                GenerateSampleData(data);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting plugin health data");
            GenerateSampleData(data);
        }

        data.ReportGeneratedAt = DateTime.UtcNow;
        return data;
    }

    public ReportMetadata GetMetadata()
    {
        return new ReportMetadata
        {
            Name = "PluginHealth",
            Description = "Plugin system health including states, memory usage, and load times",
            Category = "Diagnostics",
            SupportedFormats = new[] { ReportFormat.HTML, ReportFormat.CSV },
            DataSourcePattern = "plugin-health.json"
        };
    }

    private void GenerateSampleData(PluginHealthData data)
    {
        var plugins = new List<PluginStatus>
        {
            new()
            {
                Name = "InventoryPlugin",
                Version = "1.2.0",
                State = "Running",
                MemoryUsageMB = 12,
                LoadDuration = TimeSpan.FromMilliseconds(245),
                HealthStatusReason = null,
                DegradedSince = null
            },
            new()
            {
                Name = "StatusEffectsPlugin",
                Version = "1.1.5",
                State = "Running",
                MemoryUsageMB = 8,
                LoadDuration = TimeSpan.FromMilliseconds(189),
                HealthStatusReason = null,
                DegradedSince = null
            },
            new()
            {
                Name = "AnalyticsPlugin",
                Version = "2.0.1",
                State = "Running",
                MemoryUsageMB = 15,
                LoadDuration = TimeSpan.FromMilliseconds(312),
                HealthStatusReason = null,
                DegradedSince = null
            },
            new()
            {
                Name = "FastReportPlugin",
                Version = "1.0.0",
                State = "Running",
                MemoryUsageMB = 45,
                LoadDuration = TimeSpan.FromMilliseconds(678),
                HealthStatusReason = null,
                DegradedSince = null
            },
            new()
            {
                Name = "DungeonGeneratorPlugin",
                Version = "0.9.2",
                State = "Degraded",
                MemoryUsageMB = 28,
                LoadDuration = TimeSpan.FromMilliseconds(1250),
                HealthStatusReason = "High memory usage and slow load time",
                DegradedSince = DateTime.UtcNow.AddHours(-3)
            },
            new()
            {
                Name = "LegacyModPlugin",
                Version = "0.5.0",
                State = "Failed",
                MemoryUsageMB = 0,
                LoadDuration = TimeSpan.FromMilliseconds(120),
                HealthStatusReason = "Initialization failed",
                ErrorMessage = "Missing dependency: OldFramework.dll",
                StackTrace = "at LegacyModPlugin.Initialize()\nat PluginManager.LoadPlugin()"
            }
        };

        data.Plugins = plugins;
        data.TotalPlugins = plugins.Count;
        data.RunningPlugins = plugins.Count(p => p.State == "Running");
        data.FailedPlugins = plugins.Count(p => p.State == "Failed");
        data.DegradedPlugins = plugins.Count(p => p.State == "Degraded");
        data.SuccessRate = data.TotalPlugins > 0 
            ? (decimal)data.RunningPlugins / data.TotalPlugins * 100 
            : 0;
        data.TotalMemoryUsageMB = plugins.Sum(p => p.MemoryUsageMB);
        data.TotalLoadTime = TimeSpan.FromMilliseconds(plugins.Sum(p => p.LoadDuration.TotalMilliseconds));
    }
}
