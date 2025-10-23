using LablabBean.Contracts.Video.Models;

namespace LablabBean.Plugins.Recording.Video.Configuration;

/// <summary>
/// Configuration for video recording operations
/// </summary>
public record RecordingConfiguration
{
    /// <summary>
    /// Default recording options for game sessions
    /// </summary>
    public GameRecordingOptions GameRecording { get; init; } = new();

    /// <summary>
    /// Default recording options for manual recordings
    /// </summary>
    public ManualRecordingOptions ManualRecording { get; init; } = new();

    /// <summary>
    /// Output directory settings
    /// </summary>
    public OutputSettings Output { get; init; } = new();

    /// <summary>
    /// Auto-recording behavior
    /// </summary>
    public AutoRecordingSettings AutoRecording { get; init; } = new();
}

/// <summary>
/// Recording options for game sessions
/// </summary>
public record GameRecordingOptions
{
    /// <summary>
    /// Video codec for game recordings
    /// </summary>
    public string VideoCodec { get; init; } = "libx264";

    /// <summary>
    /// Encoding preset for game recordings
    /// </summary>
    public string Preset { get; init; } = "fast";

    /// <summary>
    /// Quality setting (CRF)
    /// </summary>
    public int Quality { get; init; } = 23;

    /// <summary>
    /// Frame rate for game recordings
    /// </summary>
    public int FrameRate { get; init; } = 30;

    /// <summary>
    /// Recording source
    /// </summary>
    public VideoRecordingSource Source { get; init; } = VideoRecordingSource.Screen;

    /// <summary>
    /// Record audio during game sessions
    /// </summary>
    public bool RecordAudio { get; init; } = false;

    /// <summary>
    /// Maximum duration for game recordings (0 = unlimited)
    /// </summary>
    public int MaxDurationSeconds { get; init; } = 3600; // 1 hour

    /// <summary>
    /// Custom resolution for game recordings
    /// </summary>
    public string? Resolution { get; init; }
}

/// <summary>
/// Recording options for manual recordings
/// </summary>
public record ManualRecordingOptions
{
    /// <summary>
    /// Video codec for manual recordings
    /// </summary>
    public string VideoCodec { get; init; } = "libx264";

    /// <summary>
    /// Encoding preset for manual recordings
    /// </summary>
    public string Preset { get; init; } = "medium";

    /// <summary>
    /// Quality setting (CRF)
    /// </summary>
    public int Quality { get; init; } = 20;

    /// <summary>
    /// Frame rate for manual recordings
    /// </summary>
    public int FrameRate { get; init; } = 30;

    /// <summary>
    /// Recording source
    /// </summary>
    public VideoRecordingSource Source { get; init; } = VideoRecordingSource.Screen;

    /// <summary>
    /// Record audio during manual recordings
    /// </summary>
    public bool RecordAudio { get; init; } = true;

    /// <summary>
    /// Maximum duration for manual recordings (0 = unlimited)
    /// </summary>
    public int MaxDurationSeconds { get; init; } = 0;

    /// <summary>
    /// Custom resolution for manual recordings
    /// </summary>
    public string? Resolution { get; init; }
}

/// <summary>
/// Output directory and file naming settings
/// </summary>
public record OutputSettings
{
    /// <summary>
    /// Base directory for recordings
    /// </summary>
    public string BaseDirectory { get; init; } = "recordings";

    /// <summary>
    /// Subdirectory for video recordings
    /// </summary>
    public string VideoSubdirectory { get; init; } = "video";

    /// <summary>
    /// File name pattern for game recordings
    /// </summary>
    public string GameRecordingPattern { get; init; } = "game_session_{timestamp}.mp4";

    /// <summary>
    /// File name pattern for manual recordings
    /// </summary>
    public string ManualRecordingPattern { get; init; } = "recording_{timestamp}.mp4";

    /// <summary>
    /// Timestamp format for file names
    /// </summary>
    public string TimestampFormat { get; init; } = "yyyyMMdd_HHmmss";

    /// <summary>
    /// Whether to create subdirectories by date
    /// </summary>
    public bool CreateDateSubdirectories { get; init; } = false;

    /// <summary>
    /// Date subdirectory format
    /// </summary>
    public string DateSubdirectoryFormat { get; init; } = "yyyy-MM-dd";
}

/// <summary>
/// Auto-recording behavior settings
/// </summary>
public record AutoRecordingSettings
{
    /// <summary>
    /// Enable automatic recording on game start
    /// </summary>
    public bool EnableAutoRecording { get; init; } = true;

    /// <summary>
    /// Minimum game session duration to trigger recording (seconds)
    /// </summary>
    public int MinimumSessionDuration { get; init; } = 10;

    /// <summary>
    /// Stop recording when game ends
    /// </summary>
    public bool StopOnGameEnd { get; init; } = true;

    /// <summary>
    /// Continue recording for X seconds after game ends
    /// </summary>
    public int PostGameRecordingSeconds { get; init; } = 0;

    /// <summary>
    /// Game modes to exclude from auto-recording
    /// </summary>
    public string[] ExcludedGameModes { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Maximum number of concurrent auto-recordings
    /// </summary>
    public int MaxConcurrentRecordings { get; init; } = 1;
}
