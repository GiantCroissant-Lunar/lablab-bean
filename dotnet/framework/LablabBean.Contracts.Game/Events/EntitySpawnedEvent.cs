using LablabBean.Contracts.Game.Models;

namespace LablabBean.Contracts.Game.Events;

/// <summary>
/// Published when an entity is spawned in the game world.
/// </summary>
public record EntitySpawnedEvent(
    Guid EntityId,
    string EntityType,
    Position Position,
    DateTimeOffset Timestamp
)
{
    /// <summary>
    /// Convenience constructor that automatically sets timestamp to current UTC time.
    /// </summary>
    public EntitySpawnedEvent(Guid entityId, string entityType, Position position)
        : this(entityId, entityType, position, DateTimeOffset.UtcNow)
    {
    }
}
