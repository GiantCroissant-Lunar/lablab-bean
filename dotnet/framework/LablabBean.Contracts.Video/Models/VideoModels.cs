namespace LablabBean.Contracts.Video.Models;

/// <summary>
/// Video playback options
/// </summary>
public record VideoPlaybackOptions
{
    /// <summary>
    /// Playback speed multiplier (default: 1.0)
    /// </summary>
    public double Speed { get; init; } = 1.0;

    /// <summary>
    /// Start position in seconds (default: 0)
    /// </summary>
    public double StartPosition { get; init; } = 0;

    /// <summary>
    /// End position in seconds (0 = play to end)
    /// </summary>
    public double EndPosition { get; init; } = 0;

    /// <summary>
    /// Loop playback
    /// </summary>
    public bool Loop { get; init; } = false;

    /// <summary>
    /// Fullscreen playback
    /// </summary>
    public bool Fullscreen { get; init; } = false;

    /// <summary>
    /// Volume level (0.0 to 1.0)
    /// </summary>
    public double Volume { get; init; } = 1.0;

    /// <summary>
    /// Mute audio
    /// </summary>
    public bool Mute { get; init; } = false;

    /// <summary>
    /// Window title for player
    /// </summary>
    public string? WindowTitle { get; init; }

    /// <summary>
    /// Additional FFplay arguments
    /// </summary>
    public string[] AdditionalArgs { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Video recording options
/// </summary>
public record VideoRecordingOptions
{
    /// <summary>
    /// Recording title/description
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Video codec (default: libx264)
    /// </summary>
    public string VideoCodec { get; init; } = "libx264";

    /// <summary>
    /// Encoding preset
    /// </summary>
    public string Preset { get; init; } = "fast";

    /// <summary>
    /// Quality setting (CRF for x264)
    /// </summary>
    public int Quality { get; init; } = 23;

    /// <summary>
    /// Frame rate
    /// </summary>
    public int FrameRate { get; init; } = 30;

    /// <summary>
    /// Resolution (e.g., "1920x1080")
    /// </summary>
    public string? Resolution { get; init; }

    /// <summary>
    /// Record audio
    /// </summary>
    public bool RecordAudio { get; init; } = false;

    /// <summary>
    /// Audio device
    /// </summary>
    public string? AudioDevice { get; init; }

    /// <summary>
    /// Maximum duration in seconds (0 = unlimited)
    /// </summary>
    public int MaxDurationSeconds { get; init; } = 0;

    /// <summary>
    /// Recording source type
    /// </summary>
    public VideoRecordingSource Source { get; init; } = VideoRecordingSource.Screen;

    /// <summary>
    /// Platform-specific settings
    /// </summary>
    public Dictionary<string, object> PlatformSettings { get; init; } = new();

    /// <summary>
    /// Additional FFmpeg arguments
    /// </summary>
    public string[] AdditionalArgs { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Video conversion options
/// </summary>
public record VideoConversionOptions
{
    /// <summary>
    /// Output video codec
    /// </summary>
    public string? VideoCodec { get; init; }

    /// <summary>
    /// Output audio codec
    /// </summary>
    public string? AudioCodec { get; init; }

    /// <summary>
    /// Output resolution
    /// </summary>
    public string? Resolution { get; init; }

    /// <summary>
    /// Output frame rate
    /// </summary>
    public int? FrameRate { get; init; }

    /// <summary>
    /// Quality setting
    /// </summary>
    public int? Quality { get; init; }

    /// <summary>
    /// Encoding preset
    /// </summary>
    public string? Preset { get; init; }

    /// <summary>
    /// Start time for trimming (seconds)
    /// </summary>
    public double? StartTime { get; init; }

    /// <summary>
    /// Duration for trimming (seconds)
    /// </summary>
    public double? Duration { get; init; }

    /// <summary>
    /// Additional FFmpeg arguments
    /// </summary>
    public string[] AdditionalArgs { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Video information
/// </summary>
public record VideoInfo
{
    /// <summary>
    /// File path
    /// </summary>
    public required string FilePath { get; init; }

    /// <summary>
    /// Duration in seconds
    /// </summary>
    public double Duration { get; init; }

    /// <summary>
    /// Video width
    /// </summary>
    public int Width { get; init; }

    /// <summary>
    /// Video height
    /// </summary>
    public int Height { get; init; }

    /// <summary>
    /// Frame rate
    /// </summary>
    public double FrameRate { get; init; }

    /// <summary>
    /// Video codec
    /// </summary>
    public string? VideoCodec { get; init; }

    /// <summary>
    /// Audio codec
    /// </summary>
    public string? AudioCodec { get; init; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; init; }

    /// <summary>
    /// Bitrate in bits per second
    /// </summary>
    public long Bitrate { get; init; }

    /// <summary>
    /// Has audio track
    /// </summary>
    public bool HasAudio { get; init; }

    /// <summary>
    /// Creation time
    /// </summary>
    public DateTime? CreationTime { get; init; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// Video session information
/// </summary>
public record VideoSession
{
    /// <summary>
    /// Session ID
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Session type
    /// </summary>
    public VideoSessionType Type { get; init; }

    /// <summary>
    /// File path being processed
    /// </summary>
    public required string FilePath { get; init; }

    /// <summary>
    /// Session start time
    /// </summary>
    public DateTime StartTime { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Process ID
    /// </summary>
    public int ProcessId { get; init; }

    /// <summary>
    /// Session status
    /// </summary>
    public VideoSessionStatus Status { get; init; } = VideoSessionStatus.Active;

    /// <summary>
    /// Progress percentage (0-100)
    /// </summary>
    public double Progress { get; init; } = 0;

    /// <summary>
    /// Additional session data
    /// </summary>
    public Dictionary<string, object> Data { get; init; } = new();
}

/// <summary>
/// Video recording source types
/// </summary>
public enum VideoRecordingSource
{
    Screen,
    Window,
    Region,
    Camera,
    Custom
}

/// <summary>
/// Video session types
/// </summary>
public enum VideoSessionType
{
    Playback,
    Recording,
    Conversion,
    Analysis
}

/// <summary>
/// Video session status
/// </summary>
public enum VideoSessionStatus
{
    Active,
    Paused,
    Completed,
    Failed,
    Cancelled
}
