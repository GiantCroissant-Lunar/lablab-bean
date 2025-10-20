using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Game.Core.Components;
using LablabBean.Game.Core.Maps;
using Microsoft.Extensions.Logging;

namespace LablabBean.Game.Core.Systems;

/// <summary>
/// System that handles entity movement
/// </summary>
public class MovementSystem
{
    private readonly ILogger<MovementSystem> _logger;

    public MovementSystem(ILogger<MovementSystem> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Attempts to move an entity to a new position
    /// Returns true if the move was successful
    /// </summary>
    public bool MoveEntity(World world, Entity entity, Position newPosition, DungeonMap map)
    {
        if (!entity.IsAlive() || !entity.Has<Position>())
        {
            return false;
        }

        // Check if the new position is walkable
        if (!map.IsWalkable(newPosition.Point))
        {
            _logger.LogDebug("Cannot move entity to {Position} - not walkable", newPosition.Point);
            return false;
        }

        // Check if another entity blocks this position
        if (IsPositionBlocked(world, newPosition))
        {
            _logger.LogDebug("Cannot move entity to {Position} - position blocked", newPosition.Point);
            return false;
        }

        // Update position
        ref var position = ref entity.Get<Position>();
        var oldPosition = position.Point;
        position.Point = newPosition.Point;

        // Update direction if entity has one
        if (entity.Has<Direction>())
        {
            ref var direction = ref entity.Get<Direction>();
            var delta = newPosition.Point - oldPosition;
            if (delta.X != 0 || delta.Y != 0)
            {
                direction.Value = SadRogue.Primitives.Direction.GetDirection(delta);
            }
        }

        _logger.LogTrace("Entity moved from {OldPos} to {NewPos}", oldPosition, newPosition.Point);
        return true;
    }

    /// <summary>
    /// Checks if a position is blocked by another entity
    /// </summary>
    private bool IsPositionBlocked(World world, Position position)
    {
        var query = new QueryDescription().WithAll<Position, BlocksMovement>();

        bool isBlocked = false;

        world.Query(in query, (Entity entity, ref Position pos, ref BlocksMovement blocks) =>
        {
            if (blocks.Blocks && pos.Point == position.Point)
            {
                isBlocked = true;
            }
        });

        return isBlocked;
    }

    /// <summary>
    /// Applies velocity to entities
    /// </summary>
    public void ApplyVelocity(World world, DungeonMap map)
    {
        var query = new QueryDescription().WithAll<Position, Velocity>();

        var movements = new List<(Entity entity, Position newPosition)>();

        // Collect all movements first
        world.Query(in query, (Entity entity, ref Position pos, ref Velocity vel) =>
        {
            var newPos = new Position(pos.Point + vel.Delta);
            movements.Add((entity, newPos));
        });

        // Apply movements
        foreach (var (entity, newPosition) in movements)
        {
            MoveEntity(world, entity, newPosition, map);
        }
    }

    /// <summary>
    /// Gets the entity at a specific position, if any
    /// </summary>
    public Entity? GetEntityAtPosition(World world, Position position)
    {
        var query = new QueryDescription().WithAll<Position>();

        Entity? result = null;

        world.Query(in query, (Entity entity, ref Position pos) =>
        {
            if (pos.Point == position.Point)
            {
                result = entity;
            }
        });

        return result;
    }
}
