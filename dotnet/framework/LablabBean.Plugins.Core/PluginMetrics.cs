using System.Diagnostics;

namespace LablabBean.Plugins.Core;

/// <summary>
/// Tracks metrics for plugin loading and execution
/// </summary>
public class PluginMetrics
{
    public string PluginName { get; init; } = string.Empty;
    public DateTime LoadStartTime { get; init; }
    public DateTime? LoadEndTime { get; set; }
    public TimeSpan? LoadDuration => LoadEndTime.HasValue ? LoadEndTime.Value - LoadStartTime : null;
    public bool LoadedSuccessfully { get; set; }
    public string? LoadError { get; set; }
    public long MemoryBeforeLoad { get; init; }
    public long? MemoryAfterLoad { get; set; }
    public long? MemoryDelta => MemoryAfterLoad.HasValue ? MemoryAfterLoad.Value - MemoryBeforeLoad : null;
    public int DependencyCount { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Profile { get; set; } = string.Empty;
}

/// <summary>
/// Aggregates metrics across all plugins
/// </summary>
public class PluginSystemMetrics
{
    private readonly List<PluginMetrics> _pluginMetrics = new();
    private readonly Stopwatch _systemStopwatch = new();

    public DateTime SystemStartTime { get; private set; }
    public DateTime? SystemReadyTime { get; private set; }
    public TimeSpan? TotalLoadTime => SystemReadyTime.HasValue ? SystemReadyTime.Value - SystemStartTime : null;

    public int TotalPluginsAttempted => _pluginMetrics.Count;
    public int TotalPluginsLoaded => _pluginMetrics.Count(m => m.LoadedSuccessfully);
    public int TotalPluginsFailed => _pluginMetrics.Count(m => !m.LoadedSuccessfully);
    public double SuccessRate => TotalPluginsAttempted > 0 ? (double)TotalPluginsLoaded / TotalPluginsAttempted * 100 : 0;

    public long TotalMemoryUsed => _pluginMetrics.Sum(m => m.MemoryDelta ?? 0);
    public TimeSpan AverageLoadTime => TotalPluginsLoaded > 0
        ? TimeSpan.FromMilliseconds(_pluginMetrics.Where(m => m.LoadedSuccessfully).Average(m => m.LoadDuration?.TotalMilliseconds ?? 0))
        : TimeSpan.Zero;

    public IReadOnlyList<PluginMetrics> Plugins => _pluginMetrics.AsReadOnly();

    public void StartSystem()
    {
        SystemStartTime = DateTime.UtcNow;
        _systemStopwatch.Start();
    }

    public void CompleteSystem()
    {
        SystemReadyTime = DateTime.UtcNow;
        _systemStopwatch.Stop();
    }

    public PluginMetrics StartPluginLoad(string pluginName, string profile)
    {
        var metrics = new PluginMetrics
        {
            PluginName = pluginName,
            LoadStartTime = DateTime.UtcNow,
            MemoryBeforeLoad = GC.GetTotalMemory(false),
            Profile = profile
        };
        _pluginMetrics.Add(metrics);
        return metrics;
    }

    public void CompletePluginLoad(PluginMetrics metrics, bool success, string? error = null)
    {
        metrics.LoadEndTime = DateTime.UtcNow;
        metrics.LoadedSuccessfully = success;
        metrics.LoadError = error;
        metrics.MemoryAfterLoad = GC.GetTotalMemory(false);
    }

    public string GetSummary()
    {
        var summary = new System.Text.StringBuilder();
        summary.AppendLine("=== Plugin System Metrics ===");
        summary.AppendLine($"Total Load Time: {TotalLoadTime?.TotalSeconds:F2}s");
        summary.AppendLine($"Plugins Attempted: {TotalPluginsAttempted}");
        summary.AppendLine($"Plugins Loaded: {TotalPluginsLoaded}");
        summary.AppendLine($"Plugins Failed: {TotalPluginsFailed}");
        summary.AppendLine($"Success Rate: {SuccessRate:F1}%");
        summary.AppendLine($"Total Memory: {TotalMemoryUsed / 1024.0 / 1024.0:F2} MB");
        summary.AppendLine($"Average Load Time: {AverageLoadTime.TotalMilliseconds:F0}ms");

        if (_pluginMetrics.Any())
        {
            summary.AppendLine("\n=== Per-Plugin Metrics ===");
            foreach (var plugin in _pluginMetrics)
            {
                var status = plugin.LoadedSuccessfully ? "✅" : "❌";
                summary.AppendLine($"{status} {plugin.PluginName} ({plugin.Profile})");
                summary.AppendLine($"   Load Time: {plugin.LoadDuration?.TotalMilliseconds:F0}ms");
                summary.AppendLine($"   Memory: {plugin.MemoryDelta / 1024.0:F0} KB");
                if (!plugin.LoadedSuccessfully)
                {
                    summary.AppendLine($"   Error: {plugin.LoadError}");
                }
            }
        }

        return summary.ToString();
    }
}
