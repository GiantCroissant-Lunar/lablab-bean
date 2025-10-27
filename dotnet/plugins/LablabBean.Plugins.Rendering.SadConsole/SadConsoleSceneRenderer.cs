using LablabBean.Rendering.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Rendering.SadConsole;

/// <summary>
/// SadConsole implementation of ISceneRenderer.
/// Provides rendering functionality for SadConsole-based UI.
/// </summary>
public class SadConsoleSceneRenderer : ISceneRenderer
{
    private readonly ILogger<SadConsoleSceneRenderer> _logger;
    private Palette? _currentPalette;

    public SadConsoleSceneRenderer(ILogger<SadConsoleSceneRenderer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogDebug("SadConsoleSceneRenderer created");
    }

    public Task InitializeAsync(Palette palette, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initializing SadConsoleSceneRenderer with palette");
        _currentPalette = palette;
        // TODO: Initialize SadConsole rendering context
        return Task.CompletedTask;
    }

    public Task RenderAsync(TileBuffer buffer, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("RenderAsync called: {Width}x{Height}", buffer.Width, buffer.Height);

        // TODO: Render TileBuffer to SadConsole surface
        // This will be implemented when wired into the SadConsole UI adapter

        return Task.CompletedTask;
    }
}
