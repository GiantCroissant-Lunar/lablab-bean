using LablabBean.Contracts.UI.Services;
using LablabBean.Contracts.Game.UI;
using LablabBean.Game.TerminalUI;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Rendering.Terminal;
using LablabBean.Rendering.Contracts;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Reflection;
using TGui = global::Terminal.Gui;

namespace LablabBean.Plugins.UI.Terminal;

/// <summary>
/// Terminal UI plugin that hosts Terminal.Gui and wires up TerminalUiAdapter.
/// Loads last by depending on core gameplay and rendering plugins.
/// </summary>
public class TerminalUiPlugin : IPlugin
{
    private ILogger? _logger;
    private IPluginContext? _context;
    private bool _initialized;
    private bool _running;
    private TerminalUiAdapter? _uiAdapter;
    private TerminalSceneRenderer? _sceneRenderer;

    public string Id => "ui-terminal";
    public string Name => "Terminal UI";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;
        _context = context;
        _logger.LogInformation("Initializing Terminal UI plugin");

        // Create scene renderer - using a wrapper to adapt ILogger to ILogger<T>
        _sceneRenderer = new TerminalSceneRenderer(new LoggerAdapter<TerminalSceneRenderer>(_logger));

        // Create and register UI adapter
        _uiAdapter = new TerminalUiAdapter(_sceneRenderer, _logger);

        // Register services with plugin registry
        context.Registry.Register<IService>(_uiAdapter);
        context.Registry.Register<IDungeonCrawlerUI>(_uiAdapter);
        context.Registry.Register<ISceneRenderer>(_sceneRenderer);

        _logger.LogInformation("Registered IService, IDungeonCrawlerUI, and ISceneRenderer");

        _initialized = true;
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

    public Task StartAsync(CancellationToken ct = default)
    {
        if (!_initialized || _uiAdapter == null)
            throw new InvalidOperationException("UI plugin not initialized");

        _logger?.LogInformation("Starting Terminal.Gui UI");

        try
        {
            // Skip UI when running CLI plugin commands to avoid blocking
            var args = Environment.GetCommandLineArgs();
            if (args.Any(a => string.Equals(a, "plugins", StringComparison.OrdinalIgnoreCase)))
            {
                _logger?.LogInformation("Detected 'plugins' CLI mode; skipping UI startup.");
                return Task.CompletedTask;
            }

            TGui.Application.Init();

            // Initialize the UI adapter with Terminal.Gui
            _uiAdapter.Initialize();

            // Get the main window from the adapter
            var mainWindow = _uiAdapter.GetMainWindow();

            if (TGui.Application.Top != null)
            {
                TGui.Application.Top.Add(mainWindow);
            }

            _running = true;
            TGui.Application.Run(mainWindow);
            _running = false;
        }
        catch (ReflectionTypeLoadException)
        {
            _logger?.LogWarning("Terminal.Gui initialization failed due to type loading issues. UI plugin will not start.");
        }
        catch (TypeLoadException)
        {
            _logger?.LogWarning("Terminal.Gui initialization failed due to missing types. UI plugin will not start.");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Terminal.Gui failed to start");
        }
        finally
        {
            try { TGui.Application.Shutdown(); } catch { }
            _logger?.LogInformation("Terminal UI plugin stopped");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        if (_running)
        {
            try { TGui.Application.RequestStop(); } catch { }
        }
        return Task.CompletedTask;
    }
}
