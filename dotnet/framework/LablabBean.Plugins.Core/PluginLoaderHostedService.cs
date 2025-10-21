namespace LablabBean.Plugins.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Background hosted service for plugin discovery and loading.
/// Reads plugin paths from configuration and loads plugins on startup.
/// </summary>
public sealed class PluginLoaderHostedService : IHostedService, IDisposable
{
    private readonly ILogger<PluginLoaderHostedService> _logger;
    private readonly IConfiguration _configuration;
    private readonly PluginLoader _pluginLoader;

    public PluginLoaderHostedService(
        ILogger<PluginLoaderHostedService> logger,
        IConfiguration configuration,
        PluginLoader pluginLoader)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _pluginLoader = pluginLoader ?? throw new ArgumentNullException(nameof(pluginLoader));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting plugin loader service");

        var pluginPathsConfig = _configuration.GetSection("Plugins:Paths").Get<string[]>();
        var pluginPaths = pluginPathsConfig ?? Array.Empty<string>();

        if (pluginPaths.Length == 0)
        {
            var defaultPath = _configuration["Plugins:DefaultPath"] ?? "plugins";
            pluginPaths = new[] { defaultPath };
            _logger.LogInformation("Using default plugin path: {DefaultPath}", defaultPath);
        }

        var expandedPaths = pluginPaths
            .Select(Environment.ExpandEnvironmentVariables)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();

        if (expandedPaths.Count == 0)
        {
            _logger.LogWarning("No plugin paths configured. Plugins will not be loaded.");
            return;
        }

        _logger.LogInformation("Loading plugins from {PathCount} path(s): {Paths}", 
            expandedPaths.Count, 
            string.Join(", ", expandedPaths));

        try
        {
            var loadedCount = await _pluginLoader.DiscoverAndLoadAsync(expandedPaths, cancellationToken);
            _logger.LogInformation("Plugin loader service started. Loaded {LoadedCount} plugin(s)", loadedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load plugins during startup");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping plugin loader service");

        try
        {
            await _pluginLoader.UnloadAllAsync(cancellationToken);
            _logger.LogInformation("Plugin loader service stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during plugin unload");
        }
    }

    public void Dispose()
    {
        _pluginLoader.Dispose();
    }
}
