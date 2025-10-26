namespace LablabBean.Contracts.Media;

/// <summary>
/// Interface for detecting terminal capabilities to select optimal media renderer.
/// Implementations check environment variables, query terminal via escape sequences,
/// and probe for specific graphics protocol support.
/// </summary>
/// <remarks>
/// Terminal capability detection is performed once at application startup and results
/// are cached for the duration of the session (terminal capabilities don't change).
///
/// Detection Methods (in order of reliability):
/// 1. Environment variables (TERM, COLORTERM, TERM_PROGRAM) - Fast but may be inaccurate
/// 2. Device Attributes query (DA1) - Reliable but requires terminal response (100ms timeout)
/// 3. Terminfo database (Linux/Unix) - Accurate but platform-specific
/// 4. Fallback assumptions - Universal ASCII/braille works everywhere
///
/// Example Implementation:
/// <code>
/// public class TerminalCapabilityDetector : ITerminalCapabilityDetector
/// {
///     public TerminalInfo DetectCapabilities()
///     {
///         var capabilities = TerminalCapability.None;
///         var term = Environment.GetEnvironmentVariable("TERM") ?? "";
///
///         // Check for Kitty terminal
///         if (term == "xterm-kitty")
///             capabilities |= TerminalCapability.KittyGraphics | TerminalCapability.TrueColor;
///
///         // Check for SIXEL support
///         if (term.Contains("xterm") && ProbeSixelSupport())
///             capabilities |= TerminalCapability.Sixel;
///
///         // Unicode support (for braille)
///         if (Console.OutputEncoding.EncodingName.Contains("UTF"))
///             capabilities |= TerminalCapability.UnicodeBlockDrawing;
///
///         return new TerminalInfo(term, capabilities, Console.WindowWidth, Console.WindowHeight, GetColorCount(capabilities));
///     }
///
///     private bool ProbeSixelSupport()
///     {
///         Console.Write("\x1b[c");  // Send DA1 query
///         // Read response with 100ms timeout
///         // Check for ";4" parameter indicating SIXEL
///     }
/// }
/// </code>
/// </remarks>
public interface ITerminalCapabilityDetector
{
    /// <summary>
    /// Detect current terminal's capabilities (graphics protocols, color support, dimensions)
    /// </summary>
    /// <returns>Terminal metadata including capabilities and dimensions</returns>
    /// <remarks>
    /// This method is called once during MediaService initialization.
    /// Results should be cached by the implementation.
    ///
    /// Detection strategy:
    /// 1. Check environment variables (instant)
    /// 2. Query terminal via escape sequences (100-200ms with timeout)
    /// 3. Apply heuristics and fallbacks
    /// 4. Always include at least one fallback capability (braille or ASCII)
    ///
    /// Performance:
    /// - Fast path (env vars only): < 1ms
    /// - With probing: 100-200ms (acceptable during startup)
    /// - Timeout if terminal doesn't respond: 100ms
    ///
    /// Thread safety: Safe to call from any thread (reads only environment)
    /// </remarks>
    TerminalInfo DetectCapabilities();

    /// <summary>
    /// Check if terminal supports a specific capability
    /// </summary>
    /// <param name="capability">Capability flag to check</param>
    /// <returns>True if terminal supports this capability</returns>
    /// <remarks>
    /// Convenience method that checks the cached TerminalInfo.
    /// Equivalent to: DetectCapabilities().Capabilities.HasFlag(capability)
    ///
    /// Example:
    /// <code>
    /// if (detector.SupportsCapability(TerminalCapability.Sixel))
    /// {
    ///     // Use SIXEL renderer
    /// }
    /// </code>
    /// </remarks>
    bool SupportsCapability(TerminalCapability capability);

    /// <summary>
    /// Actively probe for a specific capability by querying the terminal
    /// </summary>
    /// <param name="capability">Capability to probe for</param>
    /// <param name="ct">Cancellation token (timeout recommended: 100-200ms)</param>
    /// <returns>True if terminal confirms support for this capability</returns>
    /// <remarks>
    /// This method actively queries the terminal and waits for a response.
    /// More accurate than environment variables, but slower (100-200ms).
    ///
    /// Probing methods by capability:
    /// - SIXEL: Send DA1 query (ESC [ c), check for parameter 4
    /// - Kitty: Send graphics query, check for valid response
    /// - TrueColor: Already known from COLORTERM variable
    ///
    /// Use cases:
    /// - Verifying ambiguous environment variables
    /// - Fallback when environment detection is inconclusive
    /// - Testing terminal in development/debugging
    ///
    /// Timeout handling:
    /// - MUST use cancellation token with timeout (100-200ms)
    /// - If timeout, return false (assume not supported)
    /// - Terminal may not respond if it doesn't understand query
    ///
    /// Example:
    /// <code>
    /// using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
    /// bool hasSixel = await detector.ProbeCapabilityAsync(TerminalCapability.Sixel, cts.Token);
    /// </code>
    /// </remarks>
    Task<bool> ProbeCapabilityAsync(TerminalCapability capability, CancellationToken ct = default);
}

/// <summary>
/// Terminal metadata including detected capabilities and dimensions
/// </summary>
/// <param name="TerminalType">Value of TERM environment variable (e.g., "xterm-256color", "xterm-kitty")</param>
/// <param name="Capabilities">Flags enum of detected capabilities (TrueColor, SIXEL, Kitty, Unicode, etc.)</param>
/// <param name="Width">Terminal width in characters</param>
/// <param name="Height">Terminal height in characters</param>
/// <param name="SupportsColor">Whether terminal supports any color (vs monochrome)</param>
/// <param name="ColorCount">Number of colors supported (2, 16, 256, or 16777216 for true color)</param>
public record TerminalInfo(
    string TerminalType,
    TerminalCapability Capabilities,
    int Width,
    int Height,
    bool SupportsColor,
    int ColorCount
);

/// <summary>
/// Terminal capability flags (combinable with bitwise OR)
/// </summary>
[Flags]
public enum TerminalCapability
{
    /// <summary>No special capabilities detected</summary>
    None = 0,

    /// <summary>24-bit RGB color support (16.7M colors)</summary>
    TrueColor = 1 << 0,

    /// <summary>SIXEL graphics protocol (DEC VT340 legacy)</summary>
    Sixel = 1 << 1,

    /// <summary>Kitty Graphics Protocol (modern, high-quality images)</summary>
    KittyGraphics = 1 << 2,

    /// <summary>Unicode block-drawing and braille characters</summary>
    UnicodeBlockDrawing = 1 << 3,

    /// <summary>ASCII-only mode (no Unicode or graphics)</summary>
    AsciiOnly = 1 << 4,

    /// <summary>Mouse input support (for UI interaction)</summary>
    MouseSupport = 1 << 5,

    /// <summary>OSC 8 hyperlinks (for clickable metadata links)</summary>
    Hyperlinks = 1 << 6
}
