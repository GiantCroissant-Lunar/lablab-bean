using LablabBean.Contracts.Media.DTOs;

namespace LablabBean.Contracts.Media;

/// <summary>
/// Interface for media renderer plugins that display video frames or audio visualizations
/// in terminal environments using different graphics protocols (SIXEL, Kitty, braille, etc.)
/// </summary>
/// <remarks>
/// Renderers are discovered via the plugin system and registered in the service registry.
/// The MediaService selects the optimal renderer based on priority and capability matching.
///
/// Implementation Example (Braille Renderer):
/// <code>
/// public class BrailleRenderer : IMediaRenderer
/// {
///     public string Name => "Braille/ASCII Renderer";
///     public int Priority => 10;  // Lowest priority (fallback)
///     public IEnumerable&lt;MediaFormat&gt; SupportedFormats => [MediaFormat.Video, MediaFormat.Both];
///     public IEnumerable&lt;TerminalCapability&gt; RequiredCapabilities =>
///         [TerminalCapability.UnicodeBlockDrawing];
///
///     public async Task&lt;bool&gt; CanRenderAsync(MediaInfo media, TerminalInfo terminal, CancellationToken ct)
///     {
///         // Can render if terminal supports Unicode (braille characters)
///         return terminal.Capabilities.HasFlag(TerminalCapability.UnicodeBlockDrawing);
///     }
///
///     public async Task InitializeAsync(RenderContext context, CancellationToken ct)
///     {
///         _targetView = context.TargetView;
///         _runeBuffer = new Rune[context.ViewportSize.Height, context.ViewportSize.Width];
///     }
///
///     public async Task RenderFrameAsync(MediaFrame frame, CancellationToken ct)
///     {
///         // Convert RGB pixels to braille characters
///         var runes = ConvertToBraille(frame.Data, frame.Width, frame.Height);
///
///         // Update UI on main thread
///         Application.MainLoop.Invoke(() => {
///             DrawToView(runes);
///             _targetView.SetNeedsDisplay();
///         });
///     }
/// }
/// </code>
/// </remarks>
public interface IMediaRenderer
{
    /// <summary>
    /// Human-readable renderer name for display and logging
    /// </summary>
    /// <example>"Kitty Graphics Renderer", "SIXEL Renderer", "Braille/ASCII Renderer"</example>
    string Name { get; }

    /// <summary>
    /// Renderer priority for automatic selection (higher values preferred)
    /// </summary>
    /// <remarks>
    /// Suggested priority ranges:
    /// - 100: Highest quality (Kitty Graphics Protocol)
    /// - 50-80: Medium quality (SIXEL, iTerm2 inline images)
    /// - 20-40: Color ASCII/ANSI (libcaca)
    /// - 10: Universal fallback (braille)
    /// </remarks>
    int Priority { get; }

    /// <summary>
    /// Media formats this renderer can handle (Audio, Video, or Both)
    /// </summary>
    /// <remarks>
    /// Video renderers typically support [Video, Both]
    /// Audio visualizers support [Audio, Both]
    /// </remarks>
    IEnumerable<MediaFormat> SupportedFormats { get; }

    /// <summary>
    /// Terminal capabilities required for this renderer to function
    /// </summary>
    /// <remarks>
    /// Examples:
    /// - Kitty renderer requires [TrueColor, KittyGraphics]
    /// - SIXEL renderer requires [Sixel]
    /// - Braille renderer requires [UnicodeBlockDrawing]
    /// - Universal fallback has empty requirements (always available)
    /// </remarks>
    IEnumerable<TerminalCapability> RequiredCapabilities { get; }

    /// <summary>
    /// Check if this renderer can handle the given media on the given terminal
    /// </summary>
    /// <param name="media">Media metadata to check compatibility</param>
    /// <param name="terminal">Terminal capabilities to check compatibility</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if renderer can handle this media/terminal combination</returns>
    /// <remarks>
    /// This method allows renderers to perform additional checks beyond basic capability flags.
    /// For example, checking video resolution limits or codec support.
    ///
    /// Called during renderer selection in MediaService.LoadAsync().
    /// Should complete quickly (< 10ms) to avoid delaying media loading.
    /// </remarks>
    Task<bool> CanRenderAsync(MediaInfo media, TerminalInfo terminal, CancellationToken ct = default);

    /// <summary>
    /// Initialize renderer with target view and rendering context
    /// </summary>
    /// <param name="context">Rendering context including target view and viewport size</param>
    /// <param name="ct">Cancellation token</param>
    /// <remarks>
    /// Called once after renderer is selected, before first frame is rendered.
    /// Use this to:
    /// - Allocate frame buffers
    /// - Set up color palettes
    /// - Initialize graphics protocol state
    /// - Cache viewport dimensions
    ///
    /// Should complete quickly (< 50ms) to minimize media load time.
    /// </remarks>
    Task InitializeAsync(RenderContext context, CancellationToken ct = default);

    /// <summary>
    /// Render a single media frame to the terminal
    /// </summary>
    /// <param name="frame">Decoded frame data (video pixels or audio samples)</param>
    /// <param name="ct">Cancellation token</param>
    /// <remarks>
    /// Called repeatedly during playback for each frame.
    /// Must be thread-safe (may be called from background decoding thread).
    ///
    /// Performance requirements:
    /// - Video: Must complete in < frame duration (e.g., 42ms for 24 FPS)
    /// - Audio visualizer: Must complete in < 33ms (30 Hz refresh)
    ///
    /// Thread safety:
    /// - Frame processing (pixel conversion) can run on any thread
    /// - UI updates MUST use Application.MainLoop.Invoke() for Terminal.Gui
    ///
    /// Error handling:
    /// - Transient errors: Log and skip frame
    /// - Fatal errors: Throw exception to stop playback
    /// </remarks>
    Task RenderFrameAsync(MediaFrame frame, CancellationToken ct = default);

    /// <summary>
    /// Clean up renderer resources
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <remarks>
    /// Called when playback stops or media is unloaded.
    /// Use this to:
    /// - Release frame buffers
    /// - Clear graphics protocol state
    /// - Reset terminal to default state
    ///
    /// Should be idempotent (safe to call multiple times).
    /// </remarks>
    Task CleanupAsync(CancellationToken ct = default);
}
