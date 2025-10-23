using LablabBean.Contracts.Video.Models;

namespace LablabBean.Contracts.Video.Events;

/// <summary>
/// Event fired when video playback starts
/// </summary>
public record VideoPlaybackStartedEvent
{
    public required string SessionId { get; init; }
    public required string VideoPath { get; init; }
    public VideoPlaybackOptions? Options { get; init; }
    public DateTime StartTime { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Event fired when video playback stops
/// </summary>
public record VideoPlaybackStoppedEvent
{
    public required string SessionId { get; init; }
    public required string VideoPath { get; init; }
    public DateTime StopTime { get; init; } = DateTime.UtcNow;
    public TimeSpan Duration { get; init; }
    public bool WasCompleted { get; init; } = true;
}

/// <summary>
/// Event fired when video recording starts
/// </summary>
public record VideoRecordingStartedEvent
{
    public required string SessionId { get; init; }
    public required string OutputPath { get; init; }
    public VideoRecordingOptions? Options { get; init; }
    public DateTime StartTime { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Event fired when video recording stops
/// </summary>
public record VideoRecordingStoppedEvent
{
    public required string SessionId { get; init; }
    public required string OutputPath { get; init; }
    public DateTime StopTime { get; init; } = DateTime.UtcNow;
    public TimeSpan Duration { get; init; }
    public bool WasSuccessful { get; init; } = true;
    public long? FileSize { get; init; }
}

/// <summary>
/// Event fired when video conversion starts
/// </summary>
public record VideoConversionStartedEvent
{
    public required string SessionId { get; init; }
    public required string InputPath { get; init; }
    public required string OutputPath { get; init; }
    public VideoConversionOptions? Options { get; init; }
    public DateTime StartTime { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Event fired when video conversion completes
/// </summary>
public record VideoConversionCompletedEvent
{
    public required string SessionId { get; init; }
    public required string InputPath { get; init; }
    public required string OutputPath { get; init; }
    public DateTime CompletionTime { get; init; } = DateTime.UtcNow;
    public TimeSpan Duration { get; init; }
    public bool WasSuccessful { get; init; } = true;
    public long? OutputFileSize { get; init; }
}

/// <summary>
/// Event fired when video conversion progress updates
/// </summary>
public record VideoConversionProgressEvent
{
    public required string SessionId { get; init; }
    public double ProgressPercentage { get; init; }
    public TimeSpan ElapsedTime { get; init; }
    public TimeSpan? EstimatedTimeRemaining { get; init; }
    public DateTime UpdateTime { get; init; } = DateTime.UtcNow;
}
