using LablabBean.Plugins.Contracts;
using LablabBean.Rendering.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Rendering.SadConsole;

/// <summary>
/// SadConsole rendering plugin that provides ISceneRenderer for Windows-based rendering.
/// </summary>
public class SadConsoleRenderingPlugin : IPlugin
{
    private ILogger? _logger;
    private IPluginContext? _context;
    private SadConsoleSceneRenderer? _sceneRenderer;

    public string Id => "rendering-sadconsole";
    public string Name => "SadConsole Rendering";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;
        _context = context;
        _logger.LogInformation("Initializing SadConsole Rendering plugin");

        // Create scene renderer
        _sceneRenderer = new SadConsoleSceneRenderer(new LoggerAdapter<SadConsoleSceneRenderer>(_logger));

        // Register with plugin registry
        context.Registry.Register<ISceneRenderer>(_sceneRenderer);

        _logger.LogInformation("Registered ISceneRenderer for SadConsole");
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("SadConsole rendering plugin started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("SadConsole rendering plugin stopped");
        return Task.CompletedTask;
    }

    // Adapter to convert ILogger to ILogger<T>
    private class LoggerAdapter<T> : ILogger<T>
    {
        private readonly ILogger _logger;

        public LoggerAdapter(ILogger logger) => _logger = logger;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _logger.BeginScope(state);
        public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => _logger.Log(logLevel, eventId, state, exception, formatter);
    }
}
