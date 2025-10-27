using LablabBean.Contracts.Game.UI;
using LablabBean.Contracts.UI.Services;
using LablabBean.Game.SadConsole;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Rendering.SadConsole;
using LablabBean.Rendering.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.UI.SadConsole;

/// <summary>
/// SadConsole UI plugin that hosts SadConsole and wires up SadConsoleUiAdapter.
/// Loads after rendering plugin and provides UI services.
/// </summary>
public class SadConsoleUiPlugin : IPlugin
{
    private ILogger? _logger;
    private IPluginContext? _context;
    private bool _initialized;
    private SadConsoleUiAdapter? _uiAdapter;
    private SadConsoleSceneRenderer? _sceneRenderer;

    public string Id => "ui-sadconsole";
    public string Name => "SadConsole UI";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;
        _context = context;
        _logger.LogInformation("Initializing SadConsole UI plugin");

        // Get scene renderer from registry
        var services = context.Registry.GetAll<ISceneRenderer>();
        _sceneRenderer = services.OfType<SadConsoleSceneRenderer>().FirstOrDefault();
        if (_sceneRenderer == null)
        {
            throw new InvalidOperationException("SadConsoleSceneRenderer not found in registry. Ensure rendering-sadconsole plugin is loaded first.");
        }

        // Create and register UI adapter
        _uiAdapter = new SadConsoleUiAdapter(_sceneRenderer, new LoggerAdapter<SadConsoleUiAdapter>(_logger));

        // Register services with plugin registry
        context.Registry.Register<IService>(_uiAdapter);
        context.Registry.Register<IDungeonCrawlerUI>(_uiAdapter);

        _logger.LogInformation("Registered IService and IDungeonCrawlerUI for SadConsole");

        _initialized = true;
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        if (!_initialized || _uiAdapter == null)
            throw new InvalidOperationException("UI plugin not initialized");

        _logger?.LogInformation("Starting SadConsole UI");

        try
        {
            // Initialize the UI adapter
            // Note: Actual SadConsole Game.Run() is handled by LablabBean.Windows host
            _uiAdapter.Initialize();
            _logger?.LogInformation("SadConsole UI adapter initialized");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "SadConsole UI failed to start");
            throw;
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("SadConsole UI plugin stopped");
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
