using LablabBean.Contracts.UI.Services;
using LablabBean.Contracts.Game.UI;
using LablabBean.Contracts.Game.UI.Services;
using LablabBean.Game.TerminalUI;
using LablabBean.Game.TerminalUI.Styles;
using LablabBean.Plugins.Contracts;
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
public partial class TerminalUiPlugin : IPlugin
{
    private ILogger? _logger;
    private IPluginContext? _context;
    private bool _initialized;
    private bool _running;
    private TerminalUiAdapter? _uiAdapter;
    private ISceneRenderer? _sceneRenderer;

    public string Id => "ui-terminal";
    public string Name => "Terminal UI";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;
        _context = context;
        _logger.LogInformation("Initializing Terminal UI plugin");

        // Resolve scene renderer from registry (must be provided by rendering-terminal plugin)
        var renderers = context.Registry.GetAll<ISceneRenderer>();
        _sceneRenderer = renderers.FirstOrDefault();
        if (_sceneRenderer == null)
        {
            throw new InvalidOperationException("No ISceneRenderer found. Ensure 'rendering-terminal' plugin is loaded first.");
        }

        // Create styles from configuration (optional overrides)
        var styles = BuildStylesFromConfig(context.Configuration);

        // Create UI adapter and register UI services
        _uiAdapter = new TerminalUiAdapter(_sceneRenderer, _logger, styles);

        // Bind activity log if available
        var activityLogs = context.Registry.GetAll<IActivityLog>();
        var activityLog = activityLogs.FirstOrDefault();
        if (activityLog != null)
        {
            _uiAdapter.BindActivityLog(activityLog);
        }

        context.Registry.Register<IService>(_uiAdapter);
        context.Registry.Register<IDungeonCrawlerUI>(_uiAdapter);

        _logger.LogInformation("Registered IService and IDungeonCrawlerUI");

        // Register binder service for external rebind triggers
        context.Registry.Register<IRenderTargetBinder>(new TerminalRenderTargetBinder(this));

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

            // If the renderer supports Terminal render target, set it to the world view
            if (_sceneRenderer is LablabBean.Plugins.Rendering.Terminal.TerminalSceneRenderer terminalRenderer)
            {
                var renderView = _uiAdapter.GetWorldRenderView();
                if (renderView != null)
                {
                    terminalRenderer.SetRenderTarget(renderView);
                }
            }

            // Get the main window from the adapter
            var mainWindow = _uiAdapter.GetMainWindow();

            if (TGui.Application.Top != null)
            {
                TGui.Application.Top.Add(mainWindow);
            }

            // Rebind renderer target initially and on layout changes
            RebindRendererTarget();
            mainWindow.LayoutComplete += (s, e) =>
            {
                // Layout changes may alter sizes; ensure renderer is bound to the right view
                RebindRendererTarget();
            };

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

public partial class TerminalUiPlugin
{
    private void RebindRendererTarget()
    {
        if (_sceneRenderer is LablabBean.Plugins.Rendering.Terminal.TerminalSceneRenderer terminalRenderer && _uiAdapter != null)
        {
            var renderView = _uiAdapter.GetWorldRenderView();
            if (renderView != null)
            {
                terminalRenderer.SetRenderTarget(renderView);
                _logger?.LogDebug("Terminal renderer target rebound to world render view");
            }
        }
    }
}

internal sealed class TerminalRenderTargetBinder : IRenderTargetBinder
{
    private readonly TerminalUiPlugin _plugin;
    public TerminalRenderTargetBinder(TerminalUiPlugin plugin) => _plugin = plugin;
    public string UiId => _plugin.Id;
    public void Rebind() => _plugin.GetType()
        .GetMethod("RebindRendererTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        ?.Invoke(_plugin, null);
}

internal static class TerminalStyleConfig
{
    public static uint? ParseColor(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        s = s.Trim();
        try
        {
            if (s.StartsWith("#"))
            {
                // #RRGGBB or #AARRGGBB
                var hex = s.Substring(1);
                if (hex.Length == 6)
                {
                    // assume opaque
                    return 0xFF000000u | Convert.ToUInt32(hex, 16);
                }
                if (hex.Length == 8)
                {
                    return Convert.ToUInt32(hex, 16);
                }
            }
            else if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return Convert.ToUInt32(s.Substring(2), 16);
            }
            else
            {
                // decimal ARGB
                return Convert.ToUInt32(s);
            }
        }
        catch { }
        return null;
    }
}

partial class TerminalUiPlugin
{
    private TerminalRenderStyles BuildStylesFromConfig(Microsoft.Extensions.Configuration.IConfiguration config)
    {
        var styles = TerminalRenderStyles.Default();
        var basePath = "Rendering:Terminal:Styles:";

        void Apply(string key, ref TerminalRenderStyles.Style style)
        {
            var fg = TerminalStyleConfig.ParseColor(config[$"{basePath}{key}:Foreground"]);
            var bg = TerminalStyleConfig.ParseColor(config[$"{basePath}{key}:Background"]);
            var glyphStr = config[$"{basePath}{key}:Glyph"];
            var glyph = !string.IsNullOrEmpty(glyphStr) ? glyphStr[0] : style.Glyph;
            var newFg = fg ?? style.ForegroundArgb;
            var newBg = bg ?? style.BackgroundArgb;
            style = new TerminalRenderStyles.Style(glyph, newFg, newBg);
        }

        var floor = styles.Floor; Apply("Floor", ref floor); styles = styles with { Floor = floor };
        var wall = styles.Wall; Apply("Wall", ref wall); styles = styles with { Wall = wall };
        var floorExp = styles.FloorExplored; Apply("FloorExplored", ref floorExp); styles = styles with { FloorExplored = floorExp };
        var wallExp = styles.WallExplored; Apply("WallExplored", ref wallExp); styles = styles with { WallExplored = wallExp };
        var entity = styles.EntityDefault; Apply("EntityDefault", ref entity); styles = styles with { EntityDefault = entity };

        // Palette override (comma-separated color values)
        var paletteCsv = config["Rendering:Terminal:Palette"];
        if (!string.IsNullOrWhiteSpace(paletteCsv))
        {
            var parts = paletteCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var list = new List<uint>();
            foreach (var p in parts)
            {
                var col = TerminalStyleConfig.ParseColor(p);
                if (col.HasValue) list.Add(col.Value);
            }
            if (list.Count > 0)
            {
                styles.Palette = list;
            }
        }

        // Fallback to defaults if not provided
        return styles;
    }
}
