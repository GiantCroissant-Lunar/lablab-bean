using LablabBean.Contracts.Media;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.MediaPlayer.Core.Detectors;

/// <summary>
/// Detects terminal capabilities for renderer selection
/// </summary>
public class TerminalCapabilityDetector : ITerminalCapabilityDetector
{
    private readonly ILogger<TerminalCapabilityDetector> _logger;
    private TerminalInfo? _cachedInfo;

    public TerminalCapabilityDetector(ILogger<TerminalCapabilityDetector> logger)
    {
        _logger = logger;
    }

    public TerminalInfo DetectCapabilities()
    {
        if (_cachedInfo != null)
            return _cachedInfo;

        _logger.LogInformation("Detecting terminal capabilities...");

        var capabilities = TerminalCapability.None;

        // Get terminal type from environment
        var termEnv = Environment.GetEnvironmentVariable("TERM") ?? "unknown";
        var colorTermEnv = Environment.GetEnvironmentVariable("COLORTERM") ?? "";
        var termProgramEnv = Environment.GetEnvironmentVariable("TERM_PROGRAM") ?? "";

        _logger.LogDebug("Environment: TERM={Term}, COLORTERM={ColorTerm}, TERM_PROGRAM={TermProgram}",
            termEnv, colorTermEnv, termProgramEnv);

        // Check for true color support
        if (colorTermEnv.Contains("truecolor", StringComparison.OrdinalIgnoreCase) ||
            colorTermEnv.Contains("24bit", StringComparison.OrdinalIgnoreCase))
        {
            capabilities |= TerminalCapability.TrueColor;
            _logger.LogDebug("Detected TrueColor support");
        }

        // Check for Kitty terminal
        if (termEnv == "xterm-kitty" || termProgramEnv == "kitty")
        {
            capabilities |= TerminalCapability.KittyGraphics;
            capabilities |= TerminalCapability.TrueColor;
            _logger.LogDebug("Detected Kitty terminal");
        }

        // Check for SIXEL support (common in VT340-compatible terminals)
        if (termEnv.Contains("mlterm", StringComparison.OrdinalIgnoreCase) ||
            termEnv.Contains("yaft", StringComparison.OrdinalIgnoreCase) ||
            termProgramEnv == "WezTerm")
        {
            capabilities |= TerminalCapability.Sixel;
            _logger.LogDebug("Detected SIXEL support");
        }

        // Check for Unicode support (required for braille)
        var encoding = System.Console.OutputEncoding;
        if (encoding.EncodingName.Contains("UTF", StringComparison.OrdinalIgnoreCase) ||
            encoding.CodePage == 65001) // UTF-8
        {
            capabilities |= TerminalCapability.UnicodeBlockDrawing;
            _logger.LogDebug("Detected Unicode support");
        }
        else
        {
            capabilities |= TerminalCapability.AsciiOnly;
            _logger.LogWarning("Unicode not supported, falling back to ASCII");
        }

        // Mouse support (most modern terminals)
        if (!string.IsNullOrEmpty(termEnv) && termEnv != "dumb")
        {
            capabilities |= TerminalCapability.MouseSupport;
        }

        // Hyperlink support (OSC 8)
        if (termProgramEnv == "iTerm.app" ||
            termProgramEnv == "WezTerm" ||
            termEnv == "xterm-kitty")
        {
            capabilities |= TerminalCapability.Hyperlinks;
        }

        // Get terminal dimensions
        int width, height;
        try
        {
            width = System.Console.WindowWidth;
            height = System.Console.WindowHeight;
        }
        catch
        {
            width = 80;
            height = 24;
            _logger.LogWarning("Could not detect terminal dimensions, using defaults: 80x24");
        }

        // Determine color support
        bool supportsColor = capabilities.HasFlag(TerminalCapability.TrueColor) ||
                            !capabilities.HasFlag(TerminalCapability.AsciiOnly);

        int colorCount = capabilities.HasFlag(TerminalCapability.TrueColor) ? 16777216 :
                        termEnv.Contains("256color") ? 256 :
                        supportsColor ? 16 : 2;

        _cachedInfo = new TerminalInfo(
            TerminalType: termEnv,
            Capabilities: capabilities,
            Width: width,
            Height: height,
            SupportsColor: supportsColor,
            ColorCount: colorCount
        );

        _logger.LogInformation(
            "Terminal capabilities detected: Type={Type}, Capabilities={Capabilities}, Size={Width}x{Height}, Colors={Colors}",
            termEnv, capabilities, width, height, colorCount);

        return _cachedInfo;
    }

    public bool SupportsCapability(TerminalCapability capability)
    {
        var info = DetectCapabilities();
        return info.Capabilities.HasFlag(capability);
    }

    public Task<bool> ProbeCapabilityAsync(TerminalCapability capability, CancellationToken ct = default)
    {
        // For now, just use the cached detection
        // In the future, could implement active probing via terminal queries
        return Task.FromResult(SupportsCapability(capability));
    }
}
