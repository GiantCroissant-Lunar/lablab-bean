using LablabBean.Contracts.Media;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.MediaPlayer.Terminal.Sixel;

/// <summary>
/// Plugin registration for Sixel graphics renderer
/// </summary>
public class SixelRendererPlugin : IPlugin
{
    private ILogger? _logger;

    public string Id => "media-player-terminal-sixel";
    public string Name => "Sixel Terminal Renderer";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;
        _logger.LogInformation("Initializing Sixel graphics renderer plugin");

        // Register Sixel renderer
        var renderer = new SixelRenderer();
        context.Registry.Register<IMediaRenderer>(renderer, priority: 80);

        _logger.LogInformation("Sixel graphics renderer registered successfully");
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("Sixel graphics renderer plugin started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("Sixel graphics renderer plugin stopped");
        return Task.CompletedTask;
    }
}
