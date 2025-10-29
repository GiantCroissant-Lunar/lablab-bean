using LablabBean.Rendering.Contracts;
using LablabBean.Rendering.Terminal.Kitty;
using Microsoft.Extensions.Logging;
using TGui = global::Terminal.Gui;

namespace LablabBean.Plugins.Rendering.Terminal;

/// <summary>
/// Terminal.Gui implementation of ISceneRenderer.
/// Renders glyph-based TileBuffers to a Terminal.Gui view, or uses Kitty graphics protocol for high-quality image rendering.
/// </summary>
public class TerminalSceneRenderer : ISceneRenderer
{
    private readonly ILogger<TerminalSceneRenderer> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private bool _supportsKittyGraphics;
    private readonly KittyGraphicsProtocol _kittyProtocol;
    private Palette? _palette;
    private TGui.View? _renderTarget;
    private int _kittyFailureCount;
    private const int MaxKittyFailures = 3;
    private Tileset? _tileset;
    private TileRasterizer? _rasterizer;

    public bool SupportsImageMode => _supportsKittyGraphics && _tileset != null;

    public TerminalSceneRenderer(ILogger<TerminalSceneRenderer> logger, ILoggerFactory loggerFactory, bool supportsKittyGraphics = false, Tileset? tileset = null)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _supportsKittyGraphics = supportsKittyGraphics;
        _kittyProtocol = new KittyGraphicsProtocol();
        _tileset = tileset;

        if (tileset != null)
        {
            _rasterizer = new TileRasterizer(loggerFactory.CreateLogger<TileRasterizer>());
        }
    }

    public Task InitializeAsync(Palette palette, CancellationToken ct = default)
    {
        _palette = palette;
        _logger.LogInformation("Terminal renderer initialized with palette of {ColorCount} colors", palette.ArgbColors.Count);
        return Task.CompletedTask;
    }

    public Task RenderAsync(TileBuffer buffer, CancellationToken ct = default)
    {
        // Priority 1: Image mode with Kitty graphics
        if (buffer.IsImageMode && buffer.PixelData != null && _supportsKittyGraphics)
        {
            _logger.LogDebug("Using Kitty graphics protocol for high-quality rendering");
            bool success = RenderViaKittyGraphics(buffer);

            // If Kitty rendering failed and we have glyph fallback, use it
            if (!success && buffer.IsGlyphMode && buffer.Glyphs != null)
            {
                _logger.LogWarning("Kitty rendering failed, falling back to glyph mode");
                if (_renderTarget != null && TGui.Application.Driver != null)
                {
                    RenderGlyphsToTerminal(buffer);
                }
            }

            return Task.CompletedTask;
        }

        // Priority 2: Glyph mode fallback
        if (buffer.IsGlyphMode && buffer.Glyphs != null)
        {
            _logger.LogDebug("Using glyph-based rendering");
            if (_renderTarget != null && TGui.Application.Driver != null)
            {
                RenderGlyphsToTerminal(buffer);
            }
            return Task.CompletedTask;
        }

        _logger.LogWarning("TileBuffer has no compatible rendering data");
        return Task.CompletedTask;
    }

    public void SetRenderTarget(TGui.View target)
    {
        _renderTarget = target;
    }

    private bool RenderViaKittyGraphics(TileBuffer buffer)
    {
        if (buffer.PixelData == null || buffer.PixelData.Length == 0)
        {
            _logger.LogWarning("No pixel data to render via Kitty graphics");
            return false;
        }

        try
        {
            // Position cursor using ANSI escape codes (more reliable in modern terminals)
            if (_renderTarget != null)
            {
                // ANSI cursor positioning: ESC[row;colH (1-indexed)
                Console.Write($"\x1b[{_renderTarget.Frame.Y + 1};{_renderTarget.Frame.X + 1}H");
            }
            else
            {
                Console.Write("\x1b[1;1H"); // Home position
            }

            // Encode and write Kitty escape sequence
            var escapeSequence = _kittyProtocol.Encode(buffer.PixelData, buffer.Width, buffer.Height);
            Console.Write(escapeSequence);
            Console.Out.Flush();

            _logger.LogTrace("Kitty graphics rendered: {Width}x{Height} pixels", buffer.Width, buffer.Height);

            // Reset failure count on success
            _kittyFailureCount = 0;
            return true;
        }
        catch (Exception ex)
        {
            _kittyFailureCount++;
            _logger.LogError(ex, "Failed to render via Kitty graphics (failure {Count}/{Max})",
                _kittyFailureCount, MaxKittyFailures);

            // Disable Kitty graphics after repeated failures to avoid retry loops
            if (_kittyFailureCount >= MaxKittyFailures)
            {
                _supportsKittyGraphics = false;
                _logger.LogWarning("Kitty graphics disabled after {Count} consecutive failures. " +
                    "Falling back to glyph mode permanently for this session.", MaxKittyFailures);
            }

            return false;
        }
    }

    private void RenderGlyphsToTerminal(TileBuffer buffer)
    {
        if (buffer.Glyphs == null || _renderTarget == null) return;

        var driver = TGui.Application.Driver;
        if (driver == null) return;

        int viewHeight = _renderTarget.Frame.Height;
        int viewWidth = _renderTarget.Frame.Width;

        for (int y = 0; y < buffer.Height && y < viewHeight; y++)
        {
            for (int x = 0; x < buffer.Width && x < viewWidth; x++)
            {
                var glyph = buffer.Glyphs[y, x];
                var fg = MapColorRefToTerminalColor(glyph.Foreground);
                var bg = MapColorRefToTerminalColor(glyph.Background);

                _renderTarget.Move(x, y);
                driver.SetAttribute(new TGui.Attribute(fg, bg));
                driver.AddRune(glyph.Rune);
            }
        }
    }

    private TGui.Color MapColorRefToTerminalColor(ColorRef colorRef)
    {
        if (colorRef.Argb.HasValue)
        {
            var argb = colorRef.Argb.Value;
            var r = (byte)((argb >> 16) & 0xFF);
            var g = (byte)((argb >> 8) & 0xFF);
            var b = (byte)(argb & 0xFF);
            return new TGui.Color(r, g, b);
        }

        return colorRef.Index switch
        {
            0 => TGui.Color.Black,
            1 => TGui.Color.Red,
            2 => TGui.Color.Green,
            3 => TGui.Color.Yellow,
            4 => TGui.Color.Blue,
            5 => TGui.Color.Magenta,
            6 => TGui.Color.Cyan,
            7 => TGui.Color.Gray,
            8 => TGui.Color.DarkGray,
            9 => TGui.Color.BrightRed,
            10 => TGui.Color.BrightGreen,
            11 => TGui.Color.BrightYellow,
            12 => TGui.Color.BrightBlue,
            13 => TGui.Color.BrightMagenta,
            14 => TGui.Color.BrightCyan,
            15 => TGui.Color.White,
            _ => TGui.Color.White
        };
    }
}
