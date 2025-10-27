using LablabBean.Rendering.Contracts;
using Microsoft.Extensions.Logging;
using TGui = global::Terminal.Gui;

namespace LablabBean.Plugins.Rendering.Terminal;

/// <summary>
/// Terminal.Gui implementation of ISceneRenderer.
/// Renders glyph-based TileBuffers to a Terminal.Gui view.
/// </summary>
public class TerminalSceneRenderer : ISceneRenderer
{
    private readonly ILogger<TerminalSceneRenderer> _logger;
    private Palette? _palette;
    private TGui.View? _renderTarget;

    public TerminalSceneRenderer(ILogger<TerminalSceneRenderer> logger)
    {
        _logger = logger;
    }

    public Task InitializeAsync(Palette palette, CancellationToken ct = default)
    {
        _palette = palette;
        _logger.LogInformation("Terminal renderer initialized with palette of {ColorCount} colors", palette.ArgbColors.Count);
        return Task.CompletedTask;
    }

    public Task RenderAsync(TileBuffer buffer, CancellationToken ct = default)
    {
        if (!buffer.IsGlyphMode)
        {
            _logger.LogWarning("Terminal renderer only supports glyph mode");
            return Task.CompletedTask;
        }

        if (buffer.Glyphs == null)
        {
            _logger.LogWarning("TileBuffer has no glyph data");
            return Task.CompletedTask;
        }

        if (_renderTarget != null && TGui.Application.Driver != null)
        {
            RenderGlyphsToTerminal(buffer);
        }

        return Task.CompletedTask;
    }

    public void SetRenderTarget(TGui.View target)
    {
        _renderTarget = target;
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
