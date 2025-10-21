using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.StatusEffects.Services;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.StatusEffects;

/// <summary>
/// Status Effects plugin - encapsulates all status effect system logic behind a plugin boundary.
/// Follows the tiered plugin architecture from Spec 004.
/// </summary>
public class StatusEffectsPlugin : IPlugin
{
    private ILogger? _logger;
    private IPluginHost? _host;

    public string Id => "status-effects";
    public string Name => "Status Effects System";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken cancellationToken = default)
    {
        _logger = context.Logger;
        _host = context.Host;

        _logger.LogInformation("Initializing Status Effects Plugin v{Version}", Version);

        // Create and register IStatusEffectService with the plugin registry
        var statusEffectService = new StatusEffectService(_logger);
        context.Registry.Register<IStatusEffectService>(statusEffectService, priority: 100);

        _logger.LogInformation("Registered IStatusEffectService in plugin registry");

        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Status Effects Plugin started successfully");
        _logger?.LogInformation("  ✓ IStatusEffectService available via DI");
        _logger?.LogInformation("  ✓ Apply, Remove, Process operations ready");
        _logger?.LogInformation("  ✓ Stat modifier calculations enabled");
        _logger?.LogInformation("  ✓ Event notifications ready");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Status Effects Plugin stopping gracefully");
        return Task.CompletedTask;
    }
}
