using LablabBean.Contracts.Media;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.MediaPlayer.Terminal.Kitty;

/// <summary>
/// Plugin registration for Kitty graphics protocol renderer
/// </summary>
public class KittyRendererPlugin : IPlugin
{
    private ILogger? _logger;

    public string Id => "media-player-terminal-kitty";
    public string Name => "Kitty Terminal Renderer";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;
        _logger.LogInformation("Initializing Kitty graphics protocol renderer plugin");

        // Register Kitty renderer
        var renderer = new KittyRenderer();
        context.Registry.Register<IMediaRenderer>(renderer, priority: 90);

        _logger.LogInformation("Kitty graphics protocol renderer registered successfully");
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("Kitty graphics protocol renderer plugin started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("Kitty graphics protocol renderer plugin stopped");
        return Task.CompletedTask;
    }
}
