namespace LablabBean.Contracts.Recording.Models;

/// <summary>
/// Represents a recording session
/// </summary>
public record RecordingSession
{
    /// <summary>
    /// Unique session identifier
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Output file path for the recording
    /// </summary>
    public required string OutputPath { get; init; }

    /// <summary>
    /// Optional title for the recording
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// When the recording started
    /// </summary>
    public DateTime StartTime { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Process ID of the asciinema process
    /// </summary>
    public int ProcessId { get; init; }

    /// <summary>
    /// Whether the recording is currently active
    /// </summary>
    public bool IsActive { get; init; } = true;
}

/// <summary>
/// Configuration options for recording
/// </summary>
public record RecordingOptions
{
    /// <summary>
    /// Maximum recording duration in seconds (0 = unlimited)
    /// </summary>
    public int MaxDurationSeconds { get; init; } = 0;

    /// <summary>
    /// Whether to overwrite existing files
    /// </summary>
    public bool OverwriteExisting { get; init; } = false;

    /// <summary>
    /// Additional asciinema command line options
    /// </summary>
    public string[] AdditionalOptions { get; init; } = Array.Empty<string>();
}
