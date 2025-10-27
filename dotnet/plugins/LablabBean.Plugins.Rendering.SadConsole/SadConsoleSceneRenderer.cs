using LablabBean.Rendering.Contracts;
using Microsoft.Extensions.Logging;
using SadConsole;
using SadRogue.Primitives;

namespace LablabBean.Plugins.Rendering.SadConsole;

/// <summary>
/// SadConsole implementation of ISceneRenderer.
/// Provides rendering functionality for SadConsole-based UI.
/// </summary>
public class SadConsoleSceneRenderer : ISceneRenderer
{
    private readonly ILogger<SadConsoleSceneRenderer> _logger;
    private Palette? _currentPalette;
    private ScreenSurface? _target;

    public SadConsoleSceneRenderer(ILogger<SadConsoleSceneRenderer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogDebug("SadConsoleSceneRenderer created");
    }

    public Task InitializeAsync(Palette palette, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initializing SadConsoleSceneRenderer with palette");
        _currentPalette = palette;
        return Task.CompletedTask;
    }

    public Task RenderAsync(TileBuffer buffer, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("RenderAsync called: {Width}x{Height}", buffer.Width, buffer.Height);

        if (_target == null)
        {
            _logger.LogTrace("No render target set for SadConsole renderer");
            return Task.CompletedTask;
        }

        if (!buffer.IsGlyphMode || buffer.Glyphs == null)
        {
            _logger.LogTrace("Buffer not in glyph mode or missing data");
            return Task.CompletedTask;
        }

        var height = Math.Min(buffer.Height, _target.Height);
        var width = Math.Min(buffer.Width, _target.Width);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var g = buffer.Glyphs[y, x];
                var fg = ToColor(g.Foreground);
                var bg = ToColor(g.Background);
                _target.SetGlyph(x, y, g.Rune, fg, bg);
            }
        }

        _target.IsDirty = true;

        return Task.CompletedTask;
    }

    public void SetRenderTarget(ScreenSurface surface)
    {
        _target = surface ?? throw new ArgumentNullException(nameof(surface));
    }

    private static Color ToColor(ColorRef c)
    {
        if (c.Argb.HasValue)
        {
            var argb = c.Argb.Value;
            byte a = (byte)((argb >> 24) & 0xFF);
            byte r = (byte)((argb >> 16) & 0xFF);
            byte g = (byte)((argb >> 8) & 0xFF);
            byte b = (byte)(argb & 0xFF);
            return new Color(r, g, b, a);
        }

        // Fall back to indexed palette if provided
        return Color.White;
    }
}
