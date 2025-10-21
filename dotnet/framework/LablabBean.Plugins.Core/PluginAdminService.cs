using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Core;

/// <summary>
/// Administrative operations for runtime plugin management
/// </summary>
public class PluginAdminService
{
    private readonly PluginRegistry _registry;
    private readonly PluginHealthChecker _healthChecker;
    private readonly PluginSystemMetrics _metrics;
    private readonly ILogger<PluginAdminService> _logger;

    public PluginAdminService(
        PluginRegistry registry,
        PluginHealthChecker healthChecker,
        PluginSystemMetrics metrics,
        ILogger<PluginAdminService> logger)
    {
        _registry = registry;
        _healthChecker = healthChecker;
        _metrics = metrics;
        _logger = logger;
    }

    /// <summary>
    /// Get detailed status of all plugins
    /// </summary>
    public async Task<PluginSystemStatus> GetSystemStatusAsync()
    {
        var plugins = _registry.GetAllPlugins();
        var healthChecks = await _healthChecker.CheckAllAsync();

        var pluginStatuses = plugins.Select(p =>
        {
            var health = healthChecks.FirstOrDefault(h => h.PluginName == p.Name);
            var metrics = _metrics.Plugins.FirstOrDefault(m => m.PluginName == p.Name);
            var isLoaded = p.State == LablabBean.Plugins.Contracts.PluginState.Started;

            return new PluginStatus
            {
                Name = p.Name,
                Version = p.Version,
                Profile = p.Manifest?.Id ?? string.Empty,
                IsLoaded = isLoaded,
                LoadedAt = isLoaded ? DateTime.UtcNow : null,
                LoadError = p.FailureReason,
                Health = health?.Status ?? PluginHealthStatus.Unknown,
                HealthMessage = health?.Message,
                LoadDuration = metrics?.LoadDuration,
                MemoryUsage = metrics?.MemoryDelta
            };
        }).ToList();

        return new PluginSystemStatus
        {
            TotalPlugins = plugins.Count(),
            LoadedPlugins = pluginStatuses.Count(p => p.IsLoaded),
            FailedPlugins = pluginStatuses.Count(p => !string.IsNullOrEmpty(p.LoadError)),
            SystemHealth = _healthChecker.GetSystemHealth(),
            Plugins = pluginStatuses,
            Metrics = _metrics,
            CheckedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Get status of a specific plugin
    /// </summary>
    public async Task<PluginStatus?> GetPluginStatusAsync(string pluginName)
    {
        var context = _registry.GetContext(pluginName);
        if (context == null)
            return null;

        var health = await _healthChecker.CheckPluginAsync(pluginName);
        var metrics = _metrics.Plugins.FirstOrDefault(m => m.PluginName == pluginName);

        return new PluginStatus
        {
            Name = context.Name,
            Version = context.Version,
            Profile = context.Profile,
            IsLoaded = context.LoadedAt.HasValue,
            LoadedAt = context.LoadedAt,
            LoadError = context.LoadError,
            Health = health.Status,
            HealthMessage = health.Message,
            LoadDuration = metrics?.LoadDuration,
            MemoryUsage = metrics?.MemoryDelta,
            AdditionalData = health.Data
        };
    }

    /// <summary>
    /// Unload a specific plugin (if supported)
    /// </summary>
    public Task<PluginOperationResult> UnloadPluginAsync(string pluginName)
    {
        _logger.LogInformation("Attempting to unload plugin: {PluginName}", pluginName);

        try
        {
            var context = _registry.GetContext(pluginName);
            if (context == null)
            {
                return Task.FromResult(new PluginOperationResult
                {
                    Success = false,
                    Message = "Plugin not found"
                });
            }

            // Unload the assembly load context
            context.AssemblyLoadContext?.Unload();

            _logger.LogInformation("Plugin {PluginName} unloaded successfully", pluginName);

            return Task.FromResult(new PluginOperationResult
            {
                Success = true,
                Message = "Plugin unloaded successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unload plugin: {PluginName}", pluginName);
            return Task.FromResult(new PluginOperationResult
            {
                Success = false,
                Message = $"Failed to unload: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Export metrics to JSON format
    /// </summary>
    public string ExportMetrics()
    {
        return System.Text.Json.JsonSerializer.Serialize(_metrics, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}

/// <summary>
/// Status information for a single plugin
/// </summary>
public class PluginStatus
{
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Profile { get; init; } = string.Empty;
    public bool IsLoaded { get; init; }
    public DateTime? LoadedAt { get; init; }
    public string? LoadError { get; init; }
    public PluginHealthStatus Health { get; init; }
    public string? HealthMessage { get; init; }
    public TimeSpan? LoadDuration { get; init; }
    public long? MemoryUsage { get; init; }
    public Dictionary<string, object>? AdditionalData { get; init; }
}

/// <summary>
/// Overall system status
/// </summary>
public class PluginSystemStatus
{
    public int TotalPlugins { get; init; }
    public int LoadedPlugins { get; init; }
    public int FailedPlugins { get; init; }
    public PluginHealthStatus SystemHealth { get; init; }
    public List<PluginStatus> Plugins { get; init; } = new();
    public PluginSystemMetrics Metrics { get; init; } = null!;
    public DateTime CheckedAt { get; init; }
}

/// <summary>
/// Result of a plugin operation
/// </summary>
public class PluginOperationResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public Dictionary<string, object>? Data { get; init; }
}
