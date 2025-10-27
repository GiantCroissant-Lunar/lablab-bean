using LablabBean.Contracts.Media;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.MediaPlayer.Terminal.Braille;

/// <summary>
/// Plugin registration for Braille renderer
/// </summary>
public class BrailleRendererPlugin : IPlugin
{
    private ILogger? _logger;

    public string Id => "media-player-terminal-braille";
    public string Name => "Braille Terminal Renderer";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;
        _logger.LogInformation("Initializing Braille terminal renderer plugin");

        // Register Braille renderer
        var renderer = new BrailleRenderer();
        context.Registry.Register<IMediaRenderer>(renderer, priority: 50);

        _logger.LogInformation("Braille terminal renderer registered successfully");
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("Braille terminal renderer plugin started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("Braille terminal renderer plugin stopped");
        return Task.CompletedTask;
    }
}
