namespace LablabBean.Plugins.Core;

/// <summary>
/// Health status for a plugin
/// </summary>
public enum PluginHealthStatus
{
    Healthy,
    Degraded,
    Unhealthy,
    Unknown
}

/// <summary>
/// Health check result for a plugin
/// </summary>
public class PluginHealthCheckResult
{
    public string PluginName { get; init; } = string.Empty;
    public PluginHealthStatus Status { get; init; }
    public string? Message { get; init; }
    public DateTime CheckedAt { get; init; } = DateTime.UtcNow;
    public Dictionary<string, object> Data { get; init; } = new();
}

/// <summary>
/// Performs health checks on loaded plugins
/// </summary>
public class PluginHealthChecker
{
    private readonly PluginRegistry _registry;
    private readonly Dictionary<string, PluginHealthCheckResult> _lastResults = new();

    public PluginHealthChecker(PluginRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// Check health of all loaded plugins
    /// </summary>
    public async Task<IReadOnlyList<PluginHealthCheckResult>> CheckAllAsync()
    {
        var results = new List<PluginHealthCheckResult>();
        var plugins = _registry.GetAllPlugins();

        foreach (var plugin in plugins)
        {
            var result = await CheckPluginAsync(plugin.Name);
            results.Add(result);
        }

        return results;
    }

    /// <summary>
    /// Check health of a specific plugin
    /// </summary>
    public Task<PluginHealthCheckResult> CheckPluginAsync(string pluginName)
    {
        try
        {
            var context = _registry.GetContext(pluginName);
            if (context == null)
            {
                var result = new PluginHealthCheckResult
                {
                    PluginName = pluginName,
                    Status = PluginHealthStatus.Unhealthy,
                    Message = "Plugin not found in registry"
                };
                _lastResults[pluginName] = result;
                return Task.FromResult(result);
            }

            // Basic health checks
            var isLoaded = context.LoadedAt.HasValue;
            var hasError = !string.IsNullOrEmpty(context.LoadError);

            PluginHealthStatus status;
            string? message = null;

            if (!isLoaded)
            {
                status = PluginHealthStatus.Unknown;
                message = "Plugin not yet loaded";
            }
            else if (hasError)
            {
                status = PluginHealthStatus.Unhealthy;
                message = $"Load error: {context.LoadError}";
            }
            else
            {
                status = PluginHealthStatus.Healthy;
                message = "Plugin is functioning normally";
            }

            var healthResult = new PluginHealthCheckResult
            {
                PluginName = pluginName,
                Status = status,
                Message = message,
                Data = new Dictionary<string, object>
                {
                    ["version"] = context.Version,
                    ["loadedAt"] = context.LoadedAt ?? DateTime.MinValue,
                    ["assemblyCount"] = context.AssemblyLoadContext?.Assemblies.Count() ?? 0
                }
            };

            _lastResults[pluginName] = healthResult;
            return Task.FromResult(healthResult);
        }
        catch (Exception ex)
        {
            var result = new PluginHealthCheckResult
            {
                PluginName = pluginName,
                Status = PluginHealthStatus.Unhealthy,
                Message = $"Health check failed: {ex.Message}"
            };
            _lastResults[pluginName] = result;
            return Task.FromResult(result);
        }
    }

    /// <summary>
    /// Get the last health check result for a plugin
    /// </summary>
    public PluginHealthCheckResult? GetLastResult(string pluginName)
    {
        return _lastResults.TryGetValue(pluginName, out var result) ? result : null;
    }

    /// <summary>
    /// Get system-wide health status
    /// </summary>
    public PluginHealthStatus GetSystemHealth()
    {
        if (!_lastResults.Any())
            return PluginHealthStatus.Unknown;

        var statuses = _lastResults.Values.Select(r => r.Status).ToList();

        if (statuses.Any(s => s == PluginHealthStatus.Unhealthy))
            return PluginHealthStatus.Unhealthy;

        if (statuses.Any(s => s == PluginHealthStatus.Degraded))
            return PluginHealthStatus.Degraded;

        if (statuses.All(s => s == PluginHealthStatus.Healthy))
            return PluginHealthStatus.Healthy;

        return PluginHealthStatus.Unknown;
    }
}
