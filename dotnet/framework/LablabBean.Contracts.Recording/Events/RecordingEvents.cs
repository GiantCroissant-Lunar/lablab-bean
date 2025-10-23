namespace LablabBean.Contracts.Recording.Events;

/// <summary>
/// Event fired when a recording session starts
/// </summary>
public record RecordingStartedEvent
{
    public required string SessionId { get; init; }
    public required string OutputPath { get; init; }
    public string? Title { get; init; }
    public DateTime StartTime { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Event fired when a recording session stops
/// </summary>
public record RecordingStoppedEvent
{
    public required string SessionId { get; init; }
    public required string OutputPath { get; init; }
    public DateTime StopTime { get; init; } = DateTime.UtcNow;
    public TimeSpan Duration { get; init; }
    public bool WasSuccessful { get; init; } = true;
}

/// <summary>
/// Event fired when a recording playback starts
/// </summary>
public record RecordingPlaybackStartedEvent
{
    public required string RecordingPath { get; init; }
    public double Speed { get; init; } = 1.0;
    public DateTime StartTime { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Event fired when a recording playback completes
/// </summary>
public record RecordingPlaybackCompletedEvent
{
    public required string RecordingPath { get; init; }
    public DateTime CompletionTime { get; init; } = DateTime.UtcNow;
    public bool WasSuccessful { get; init; } = true;
}
