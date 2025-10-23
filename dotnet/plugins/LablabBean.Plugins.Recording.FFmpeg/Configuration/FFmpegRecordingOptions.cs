namespace LablabBean.Plugins.Recording.FFmpeg.Configuration;

/// <summary>
/// Configuration options for FFmpeg video recording
/// </summary>
public record FFmpegRecordingOptions
{
    /// <summary>
    /// Video codec to use (default: libx264)
    /// </summary>
    public string VideoCodec { get; init; } = "libx264";

    /// <summary>
    /// Encoding preset (ultrafast, superfast, veryfast, faster, fast, medium, slow, slower, veryslow)
    /// </summary>
    public string Preset { get; init; } = "fast";

    /// <summary>
    /// Constant Rate Factor for quality (0-51, lower = better quality)
    /// </summary>
    public int CRF { get; init; } = 23;

    /// <summary>
    /// Frame rate for recording
    /// </summary>
    public int FrameRate { get; init; } = 30;

    /// <summary>
    /// Pixel format
    /// </summary>
    public string PixelFormat { get; init; } = "yuv420p";

    /// <summary>
    /// Custom resolution (e.g., "1920x1080"). If null, uses screen resolution
    /// </summary>
    public string? Resolution { get; init; }

    /// <summary>
    /// Audio recording enabled
    /// </summary>
    public bool RecordAudio { get; init; } = false;

    /// <summary>
    /// Audio device for recording (platform-specific)
    /// </summary>
    public string? AudioDevice { get; init; }

    /// <summary>
    /// Maximum recording duration in seconds (0 = unlimited)
    /// </summary>
    public int MaxDurationSeconds { get; init; } = 0;

    /// <summary>
    /// Whether to show FFmpeg output in console
    /// </summary>
    public bool ShowFFmpegOutput { get; init; } = false;

    /// <summary>
    /// Additional FFmpeg arguments
    /// </summary>
    public string[] AdditionalArgs { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Platform-specific capture settings
    /// </summary>
    public PlatformCaptureSettings PlatformSettings { get; init; } = new();
}

/// <summary>
/// Platform-specific capture configuration
/// </summary>
public record PlatformCaptureSettings
{
    /// <summary>
    /// Windows gdigrab settings
    /// </summary>
    public WindowsCaptureSettings Windows { get; init; } = new();

    /// <summary>
    /// Linux x11grab settings
    /// </summary>
    public LinuxCaptureSettings Linux { get; init; } = new();

    /// <summary>
    /// macOS avfoundation settings
    /// </summary>
    public MacOSCaptureSettings MacOS { get; init; } = new();
}

/// <summary>
/// Windows-specific capture settings
/// </summary>
public record WindowsCaptureSettings
{
    /// <summary>
    /// Capture cursor (default: true)
    /// </summary>
    public bool ShowCursor { get; init; } = true;

    /// <summary>
    /// Offset from top-left corner (x,y)
    /// </summary>
    public (int X, int Y)? Offset { get; init; }

    /// <summary>
    /// Capture specific window title
    /// </summary>
    public string? WindowTitle { get; init; }
}

/// <summary>
/// Linux-specific capture settings
/// </summary>
public record LinuxCaptureSettings
{
    /// <summary>
    /// Display to capture (default: ":0.0")
    /// </summary>
    public string Display { get; init; } = ":0.0";

    /// <summary>
    /// Follow mouse cursor
    /// </summary>
    public bool FollowMouse { get; init; } = false;

    /// <summary>
    /// Show mouse cursor
    /// </summary>
    public bool ShowCursor { get; init; } = true;
}

/// <summary>
/// macOS-specific capture settings
/// </summary>
public record MacOSCaptureSettings
{
    /// <summary>
    /// Screen capture device index (default: 1)
    /// </summary>
    public int ScreenDevice { get; init; } = 1;

    /// <summary>
    /// Capture mouse cursor
    /// </summary>
    public bool ShowCursor { get; init; } = true;

    /// <summary>
    /// Capture mouse clicks
    /// </summary>
    public bool CaptureClicks { get; init; } = false;
}
