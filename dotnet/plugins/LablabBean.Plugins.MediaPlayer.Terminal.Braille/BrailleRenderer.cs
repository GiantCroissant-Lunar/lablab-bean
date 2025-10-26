using LablabBean.Contracts.Media;
using LablabBean.Contracts.Media.DTOs;
using LablabBean.Plugins.MediaPlayer.Terminal.Braille.Converters;
using Terminal.Gui;

namespace LablabBean.Plugins.MediaPlayer.Terminal.Braille;

/// <summary>
/// Universal fallback renderer using braille characters for video display.
/// Works in any terminal that supports Unicode (no special graphics protocol required).
/// </summary>
public class BrailleRenderer : IMediaRenderer
{
    public string Name => "Braille/Unicode Renderer";
    public int Priority => 10; // Lowest priority - universal fallback

    public IEnumerable<MediaFormat> SupportedFormats => new[]
    {
        MediaFormat.Video,
        MediaFormat.Both
    };

    public IEnumerable<TerminalCapability> RequiredCapabilities => new[]
    {
        TerminalCapability.UnicodeBlockDrawing
    };

    private View? _targetView;
    private char[,]? _runeBuffer;
    private int _viewportWidth;
    private int _viewportHeight;
    private bool _isInitialized;

    public Task<bool> CanRenderAsync(MediaInfo media, TerminalInfo terminal, CancellationToken ct = default)
    {
        // Check if terminal supports Unicode
        var supportsUnicode = terminal.Capabilities.HasFlag(TerminalCapability.UnicodeBlockDrawing);

        // Check if media has video
        var hasVideo = media.Format == MediaFormat.Video || media.Format == MediaFormat.Both;

        return Task.FromResult(supportsUnicode && hasVideo);
    }

    public Task InitializeAsync(RenderContext context, CancellationToken ct = default)
    {
        _viewportWidth = context.ViewportSize.Width;
        _viewportHeight = context.ViewportSize.Height;

        // Allocate rune buffer (each char represents 2x4 pixels)
        _runeBuffer = new char[_viewportHeight, _viewportWidth];

        // Initialize with empty braille
        for (int row = 0; row < _viewportHeight; row++)
        {
            for (int col = 0; col < _viewportWidth; col++)
            {
                _runeBuffer[row, col] = (char)0x2800; // Empty braille
            }
        }

        _isInitialized = true;
        return Task.CompletedTask;
    }

    public Task RenderFrameAsync(MediaFrame frame, CancellationToken ct = default)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Renderer not initialized. Call InitializeAsync first.");

        if (frame.Type != FrameType.Video)
            return Task.CompletedTask; // Skip audio frames

        // Convert frame to braille on background thread (CPU-intensive)
        var brailleChars = BrailleConverter.ConvertToBraille(
            frame.Data,
            frame.Width,
            frame.Height,
            threshold: 128
        );

        // Copy to buffer (resize if needed)
        CopyToBuffer(brailleChars);

        // Marshal to UI thread for rendering
        Application.Invoke(() =>
        {
            DrawToTerminal();
        });

        return Task.CompletedTask;
    }

    private void CopyToBuffer(char[,] brailleChars)
    {
        if (_runeBuffer == null)
            return;

        var srcHeight = brailleChars.GetLength(0);
        var srcWidth = brailleChars.GetLength(1);

        // Calculate scaling to fit viewport
        var scaleX = (double)srcWidth / _viewportWidth;
        var scaleY = (double)srcHeight / _viewportHeight;

        for (int row = 0; row < _viewportHeight; row++)
        {
            for (int col = 0; col < _viewportWidth; col++)
            {
                var srcRow = (int)(row * scaleY);
                var srcCol = (int)(col * scaleX);

                if (srcRow < srcHeight && srcCol < srcWidth)
                {
                    _runeBuffer[row, col] = brailleChars[srcRow, srcCol];
                }
            }
        }
    }

    private void DrawToTerminal()
    {
        if (_runeBuffer == null || _targetView == null)
            return;

        try
        {
            // Draw to Terminal.Gui view
            for (int row = 0; row < _viewportHeight && row < _targetView.Frame.Height; row++)
            {
                for (int col = 0; col < _viewportWidth && col < _targetView.Frame.Width; col++)
                {
                    var rune = (System.Text.Rune)_runeBuffer[row, col];
                    _targetView.AddRune(col, row, rune);
                }
            }

            _targetView.SetNeedsDraw();
        }
        catch (Exception)
        {
            // Silently skip rendering errors (frame will be dropped)
        }
    }

    public Task CleanupAsync(CancellationToken ct = default)
    {
        _runeBuffer = null;
        _targetView = null;
        _isInitialized = false;
        return Task.CompletedTask;
    }
}
