using LablabBean.Contracts.Media;
using LablabBean.Plugins.Contracts;
using LablabBean.Rendering.Contracts;
using LablabBean.Rendering.Terminal.Contracts;
using Microsoft.Extensions.Configuration;
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

        var loggerFactory = context.Registry.IsRegistered<ILoggerFactory>()
            ? context.Registry.Get<ILoggerFactory>()
            : Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance;

        // Detect terminal capabilities
        bool supportsKitty = false;
        bool supportsSixel = false;
        bool supportsTrueColor = false;
        bool isRemoteSession = DetectRemoteSession();

        if (isRemoteSession)
        {
            _logger.LogInformation("Remote session detected (SSH or similar), capability detection may be limited");
        }

        if (context.Registry.IsRegistered<ITerminalCapabilityDetector>())
        {
            var capabilityDetector = context.Registry.Get<ITerminalCapabilityDetector>();
            var termInfo = capabilityDetector.DetectCapabilities();
            supportsKitty = termInfo.Capabilities.HasFlag(TerminalCapability.KittyGraphics);
            supportsSixel = termInfo.Capabilities.HasFlag(TerminalCapability.Sixel);
            supportsTrueColor = termInfo.Capabilities.HasFlag(TerminalCapability.TrueColor);

            _logger.LogInformation("Terminal capabilities: Kitty={Kitty}, Sixel={Sixel}, TrueColor={TrueColor}, Remote={Remote}",
                supportsKitty, supportsSixel, supportsTrueColor, isRemoteSession);

            // Validate Kitty support if terminal claims it - fallback will happen on first render failure
            if (supportsKitty && isRemoteSession)
            {
                _logger.LogWarning("Kitty graphics claimed over remote session - will verify on first render");
            }
        }
        else
        {
            _logger.LogWarning("ITerminalCapabilityDetector not available, using default capabilities");
        }

        // Load tileset if configured and Kitty is available
        Tileset? tileset = null;
        if (supportsKitty && context.Registry.IsRegistered<IConfiguration>())
        {
            var config = context.Registry.Get<IConfiguration>();
            var tilesetPath = config["Rendering:Terminal:Tileset"];
            var tileSizeStr = config["Rendering:Terminal:TileSize"];

            if (!string.IsNullOrEmpty(tilesetPath) && int.TryParse(tileSizeStr, out int tileSize))
            {
                var loader = new TilesetLoader(loggerFactory.CreateLogger<TilesetLoader>());
                tileset = loader.Load(tilesetPath, tileSize);

                if (tileset != null)
                {
                    _logger.LogInformation("Tileset loaded: {Path} ({TileCount} tiles)", tilesetPath, tileset.Tiles.Count);
                }
                else
                {
                    _logger.LogWarning("Tileset not loaded, falling back to glyph mode");
                }
            }
            else
            {
                _logger.LogInformation("Tileset not configured (Rendering:Terminal:Tileset), using glyph mode");
            }
        }

        var renderer = new TerminalSceneRenderer(
            loggerFactory.CreateLogger<TerminalSceneRenderer>(),
            loggerFactory,
            supportsKittyGraphics: supportsKitty,
            tileset: tileset);

        context.Registry.Register<ISceneRenderer>(renderer, new ServiceMetadata
        {
            Priority = 100,
            Name = "TerminalSceneRenderer",
            Version = "1.0.0"
        });
        _logger.LogInformation("Registered ISceneRenderer for Terminal.Gui (Kitty support: {KittySupport})", supportsKitty);

        var binding = new TerminalSceneRendererBinding(renderer, loggerFactory.CreateLogger<TerminalSceneRendererBinding>());
        context.Registry.Register<ITerminalRenderBinding>(binding, new ServiceMetadata
        {
            Priority = 100,
            Name = "TerminalRenderBinding",
            Version = "1.0.0"
        });
        _logger.LogInformation("Registered ITerminalRenderBinding for Terminal.Gui");

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

    private bool DetectRemoteSession()
    {
        // Check for SSH environment variables
        var sshConnection = Environment.GetEnvironmentVariable("SSH_CONNECTION");
        var sshClient = Environment.GetEnvironmentVariable("SSH_CLIENT");
        var sshTty = Environment.GetEnvironmentVariable("SSH_TTY");

        return !string.IsNullOrEmpty(sshConnection) ||
               !string.IsNullOrEmpty(sshClient) ||
               !string.IsNullOrEmpty(sshTty);
    }
}
