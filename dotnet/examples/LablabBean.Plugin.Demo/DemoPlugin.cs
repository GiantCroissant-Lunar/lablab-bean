using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugin.Demo;

/// <summary>
/// A simple demo plugin to validate the plugin system.
/// </summary>
public class DemoPlugin : IPlugin
{
    private ILogger? _logger;
    private IPluginHost? _host;

    public string Id => "demo-plugin";
    public string Name => "Demo Plugin";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken cancellationToken = default)
    {
        _logger = context.Logger;
        _host = context.Host;

        _logger.LogInformation("DemoPlugin initialized");
        _logger.LogInformation("Plugin ID: {Id}, Name: {Name}, Version: {Version}", Id, Name, Version);
        _logger.LogInformation("Configuration available: {ConfigAvailable}", context.Configuration != null);
        _logger.LogInformation("Registry available: {RegistryAvailable}", context.Registry != null);

        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("DemoPlugin started");
        _logger?.LogInformation("This is a simple test plugin demonstrating:");
        _logger?.LogInformation("  ✓ Plugin discovery and loading");
        _logger?.LogInformation("  ✓ Context initialization with logger, config, and registry");
        _logger?.LogInformation("  ✓ Lifecycle management (Initialize → Start → Stop)");
        _logger?.LogInformation("  ✓ Host communication via IPluginHost");
        _logger?.LogInformation("  ✓ AssemblyLoadContext isolation");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("DemoPlugin stopping gracefully");
        return Task.CompletedTask;
    }
}
