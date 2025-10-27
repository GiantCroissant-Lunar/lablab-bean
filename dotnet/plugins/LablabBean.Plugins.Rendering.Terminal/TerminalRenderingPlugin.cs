using LablabBean.Plugins.Contracts;
using LablabBean.Rendering.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Rendering.Terminal;

/// <summary>
/// Terminal rendering plugin that provides ISceneRenderer for Terminal.Gui.
/// </summary>
public class TerminalRenderingPlugin : IPlugin
{
    private ILogger? _logger;
    private bool _initialized;

    public string Id => "rendering-terminal";
    public string Name => "Terminal Rendering";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;
        _logger.LogInformation("Initializing Terminal Rendering plugin");

        var loggerFactory = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance;
        var renderer = new TerminalSceneRenderer(loggerFactory.CreateLogger<TerminalSceneRenderer>());

        context.Registry.Register<ISceneRenderer>(renderer);
        _logger.LogInformation("Registered ISceneRenderer for Terminal.Gui");

        _initialized = true;
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        if (!_initialized)
            throw new InvalidOperationException("Rendering plugin not initialized");

        _logger?.LogInformation("Terminal rendering plugin started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("Terminal rendering plugin stopped");
        return Task.CompletedTask;
    }
}
