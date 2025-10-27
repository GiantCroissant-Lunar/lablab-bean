using LablabBean.Contracts.Game.UI;
using LablabBean.Contracts.UI.Services;
using LablabBean.Game.SadConsole;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Rendering.SadConsole;
using LablabBean.Rendering.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LablabBean.Game.SadConsole.Styles;

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

        // Create and register UI adapter with host service provider
        var loggerFactory = context.Host.Services.GetService(typeof(ILoggerFactory)) as ILoggerFactory
            ?? throw new InvalidOperationException("ILoggerFactory not available from host services");

        var adapterLogger = loggerFactory.CreateLogger<SadConsoleUiAdapter>();
        var styles = BuildStylesFromConfig(context.Configuration);
        _uiAdapter = new SadConsoleUiAdapter(_sceneRenderer, adapterLogger, context.Host.Services, styles);

        // Register services with plugin registry
        context.Registry.Register<IService>(_uiAdapter);
        context.Registry.Register<IDungeonCrawlerUI>(_uiAdapter);
        context.Registry.Register<IRenderTargetBinder>(new SadConsoleRenderTargetBinder(this));

        // Also register in host DI for direct access from Windows app
        // This allows LablabBean.Windows to resolve it from its service provider
        if (context.Host.Services is IServiceCollection hostServices)
        {
            _logger.LogWarning("Cannot register UI adapter in host DI - service collection not accessible at runtime");
        }

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
            // Bind renderer target via helper (plugin-only binding policy)
            RebindRendererTarget();
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

public partial class SadConsoleUiPlugin
{
    /// <summary>
    /// Rebind the SadConsole renderer target to the current GameScreen's world surface.
    /// Can be called after screen recreation or size changes.
    /// </summary>
    public void RebindRendererTarget()
    {
        if (_uiAdapter == null || _sceneRenderer == null)
        {
            _logger?.LogWarning("Cannot rebind renderer target: adapter or renderer not initialized");
            return;
        }

        if (_sceneRenderer is SadConsoleSceneRenderer scRenderer)
        {
            var gs = _uiAdapter.GetGameScreen();
            var worldSurface = gs?.WorldSurface;
            if (worldSurface != null)
            {
                scRenderer.SetRenderTarget(worldSurface);
                _logger?.LogInformation("SadConsole renderer target rebound to world surface");
            }
            else
            {
                _logger?.LogWarning("World surface not available for rebinding");
            }
        }
    }
}

internal static class SadConsoleStyleConfig
{
    public static uint? ParseColor(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        s = s.Trim();
        try
        {
            if (s.StartsWith("#"))
            {
                var hex = s.Substring(1);
                if (hex.Length == 6) return 0xFF000000u | Convert.ToUInt32(hex, 16);
                if (hex.Length == 8) return Convert.ToUInt32(hex, 16);
            }
            else if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return Convert.ToUInt32(s.Substring(2), 16);
            }
            else
            {
                return Convert.ToUInt32(s);
            }
        }
        catch { }
        return null;
    }
}

partial class SadConsoleUiPlugin
{
    private SadConsoleRenderStyles BuildStylesFromConfig(Microsoft.Extensions.Configuration.IConfiguration config)
    {
        var styles = SadConsoleRenderStyles.Default();
        var basePath = "Rendering:SadConsole:Styles:";

        void Apply(string key, ref SadConsoleRenderStyles.Style style)
        {
            var fg = SadConsoleStyleConfig.ParseColor(config[$"{basePath}{key}:Foreground"]);
            var bg = SadConsoleStyleConfig.ParseColor(config[$"{basePath}{key}:Background"]);
            var glyphStr = config[$"{basePath}{key}:Glyph"];
            var glyph = !string.IsNullOrEmpty(glyphStr) ? glyphStr[0] : style.Glyph;
            var newFg = fg ?? style.ForegroundArgb;
            var newBg = bg ?? style.BackgroundArgb;
            style = new SadConsoleRenderStyles.Style(glyph, newFg, newBg);
        }

        var floor = styles.Floor; Apply("Floor", ref floor); styles = styles with { Floor = floor };
        var wall = styles.Wall; Apply("Wall", ref wall); styles = styles with { Wall = wall };
        var floorExp = styles.FloorExplored; Apply("FloorExplored", ref floorExp); styles = styles with { FloorExplored = floorExp };
        var wallExp = styles.WallExplored; Apply("WallExplored", ref wallExp); styles = styles with { WallExplored = wallExp };
        var entity = styles.EntityDefault; Apply("EntityDefault", ref entity); styles = styles with { EntityDefault = entity };

        var paletteCsv = config["Rendering:SadConsole:Palette"];
        if (!string.IsNullOrWhiteSpace(paletteCsv))
        {
            var parts = paletteCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var list = new List<uint>();
            foreach (var p in parts)
            {
                var col = SadConsoleStyleConfig.ParseColor(p);
                if (col.HasValue) list.Add(col.Value);
            }
            if (list.Count > 0) styles.Palette = list;
        }

        return styles;
    }
}

internal sealed class SadConsoleRenderTargetBinder : IRenderTargetBinder
{
    private readonly SadConsoleUiPlugin _plugin;
    public SadConsoleRenderTargetBinder(SadConsoleUiPlugin plugin) => _plugin = plugin;
    public string UiId => _plugin.Id;
    public void Rebind() => _plugin.RebindRendererTarget();
}
