using LablabBean.Contracts.Game.Models;

namespace LablabBean.Contracts.Game.Events;

/// <summary>
/// Published when an entity moves to a new position.
/// </summary>
public record EntityMovedEvent(
    Guid EntityId,
    Position OldPosition,
    Position NewPosition,
    DateTimeOffset Timestamp
)
{
    /// <summary>
    /// Convenience constructor that automatically sets timestamp to current UTC time.
    /// </summary>
    public EntityMovedEvent(Guid entityId, Position oldPosition, Position newPosition)
        : this(entityId, oldPosition, newPosition, DateTimeOffset.UtcNow)
    {
    }
}
