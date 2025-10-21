using LablabBean.Contracts.Game.Models;

namespace LablabBean.Contracts.Game.Events;

/// <summary>
/// Published when game state changes (pause, resume, game over, etc.).
/// </summary>
public record GameStateChangedEvent(
    GameStateType OldState,
    GameStateType NewState,
    string? Reason,
    DateTimeOffset Timestamp
)
{
    /// <summary>
    /// Convenience constructor that automatically sets timestamp to current UTC time.
    /// </summary>
    public GameStateChangedEvent(GameStateType oldState, GameStateType newState, string? reason = null)
        : this(oldState, newState, reason, DateTimeOffset.UtcNow)
    {
    }
}
