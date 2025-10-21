namespace LablabBean.Contracts.Game.Models;

/// <summary>
/// Immutable game state snapshot.
/// </summary>
public record GameState(
    GameStateType State,
    int TurnNumber,
    Guid? PlayerEntityId,
    int CurrentLevel,
    DateTimeOffset StartTime
);

/// <summary>
/// Game state types.
/// </summary>
public enum GameStateType
{
    NotStarted,
    Running,
    Paused,
    GameOver,
    Victory
}
