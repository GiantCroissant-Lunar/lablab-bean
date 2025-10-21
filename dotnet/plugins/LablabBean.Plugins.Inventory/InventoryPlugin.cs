using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Inventory;

/// <summary>
/// Inventory plugin - encapsulates all inventory system logic behind a plugin boundary.
/// Follows the tiered plugin architecture from Spec 004.
/// </summary>
public class InventoryPlugin : IPlugin
{
    private ILogger? _logger;
    private IPluginHost? _host;

    public string Id => "inventory";
    public string Name => "Inventory System";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken cancellationToken = default)
    {
        _logger = context.Logger;
        _host = context.Host;

        _logger.LogInformation("Initializing Inventory Plugin v{Version}", Version);

        // Create and register IInventoryService with the plugin registry
        // This makes the service available to the host and other plugins
        var inventoryService = new InventoryService(_logger);
        context.Registry.Register<IInventoryService>(inventoryService, priority: 100);

        _logger.LogInformation("Registered IInventoryService in plugin registry");

        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Inventory Plugin started successfully");
        _logger?.LogInformation("  ✓ IInventoryService available via DI");
        _logger?.LogInformation("  ✓ Pickup, Use, Equip operations ready");
        _logger?.LogInformation("  ✓ Event notifications enabled");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Inventory Plugin stopping gracefully");
        return Task.CompletedTask;
    }
}
