namespace LablabBean.Contracts.Game.Events;

/// <summary>
/// Event fired when a game session starts
/// </summary>
public record GameStartedEvent
{
    /// <summary>
    /// Unique identifier for the game session
    /// </summary>
    public required string GameId { get; init; }

    /// <summary>
    /// Game mode or type being played
    /// </summary>
    public required string GameMode { get; init; }

    /// <summary>
    /// When the game session started
    /// </summary>
    public DateTime StartTime { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Optional metadata about the game session
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Event fired when a game session ends
/// </summary>
public record GameEndedEvent
{
    /// <summary>
    /// Unique identifier for the game session
    /// </summary>
    public required string GameId { get; init; }

    /// <summary>
    /// Game mode or type that was played
    /// </summary>
    public required string GameMode { get; init; }

    /// <summary>
    /// When the game session ended
    /// </summary>
    public DateTime EndTime { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Duration of the game session
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Final score or result
    /// </summary>
    public int? FinalScore { get; init; }

    /// <summary>
    /// Whether the game ended normally or was aborted
    /// </summary>
    public bool WasCompleted { get; init; } = true;

    /// <summary>
    /// Optional metadata about the game session results
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}
