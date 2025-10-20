using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Game.Core.Components;
using LablabBean.Game.Core.Maps;
using Microsoft.Extensions.Logging;
using SadRogue.Primitives;

namespace LablabBean.Game.Core.Systems;

/// <summary>
/// System that handles AI behavior for entities
/// </summary>
public class AISystem
{
    private readonly ILogger<AISystem> _logger;
    private readonly MovementSystem _movementSystem;
    private readonly CombatSystem _combatSystem;
    private readonly Random _random;

    public AISystem(
        ILogger<AISystem> logger,
        MovementSystem movementSystem,
        CombatSystem combatSystem)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _movementSystem = movementSystem ?? throw new ArgumentNullException(nameof(movementSystem));
        _combatSystem = combatSystem ?? throw new ArgumentNullException(nameof(combatSystem));
        _random = new Random();
    }

    /// <summary>
    /// Processes AI for all entities with AI components
    /// </summary>
    public void ProcessAI(World world, DungeonMap map)
    {
        var query = new QueryDescription().WithAll<AI, Actor, Position>();

        var aiActions = new List<(Entity entity, AIBehavior behavior, Position position)>();

        // Collect all AI entities
        world.Query(in query, (Entity entity, ref AI ai, ref Actor actor, ref Position pos) =>
        {
            if (actor.CanAct)
            {
                aiActions.Add((entity, ai.Behavior, pos));
            }
        });

        // Process each AI entity
        foreach (var (entity, behavior, position) in aiActions)
        {
            ProcessEntityAI(world, entity, behavior, position, map);

            // Consume energy after acting
            if (entity.Has<Actor>())
            {
                ref var actor = ref entity.Get<Actor>();
                actor.ConsumeEnergy();
            }
        }
    }

    /// <summary>
    /// Processes AI for a single entity
    /// </summary>
    private void ProcessEntityAI(World world, Entity entity, AIBehavior behavior, Position position, DungeonMap map)
    {
        switch (behavior)
        {
            case AIBehavior.Wander:
                Wander(world, entity, position, map);
                break;

            case AIBehavior.Chase:
                Chase(world, entity, position, map);
                break;

            case AIBehavior.Flee:
                Flee(world, entity, position, map);
                break;

            case AIBehavior.Patrol:
                Patrol(world, entity, position, map);
                break;

            case AIBehavior.Idle:
                // Do nothing
                break;
        }
    }

    /// <summary>
    /// AI behavior: Wander randomly
    /// </summary>
    private void Wander(World world, Entity entity, Position position, DungeonMap map)
    {
        // Pick a random walkable neighbor
        var neighbors = map.GetWalkableNeighbors(position.Point).ToList();

        if (neighbors.Count > 0)
        {
            var targetPos = neighbors[_random.Next(neighbors.Count)];
            var newPosition = new Position(targetPos);

            _movementSystem.MoveEntity(world, entity, newPosition, map);
        }
    }

    /// <summary>
    /// AI behavior: Chase the player
    /// </summary>
    private void Chase(World world, Entity entity, Position position, DungeonMap map)
    {
        var playerPos = GetPlayerPosition(world);

        if (playerPos == null)
        {
            Wander(world, entity, position, map);
            return;
        }

        // Check if player is adjacent (can attack)
        if (map.GetDistance(position.Point, playerPos.Value.Point) <= 1.5)
        {
            var player = GetPlayer(world);
            if (player.HasValue)
            {
                _combatSystem.Attack(world, entity, player.Value);
                return;
            }
        }

        // Otherwise, move towards player
        var path = map.FindPath(position.Point, playerPos.Value.Point);

        if (path != null && path.Length > 1)
        {
            // Move to the next step in the path
            var nextStep = path.Steps.ElementAt(1);
            var newPosition = new Position(nextStep);

            if (!_movementSystem.MoveEntity(world, entity, newPosition, map))
            {
                // If movement failed, try wandering
                Wander(world, entity, position, map);
            }
        }
        else
        {
            // No path found, wander
            Wander(world, entity, position, map);
        }
    }

    /// <summary>
    /// AI behavior: Flee from the player
    /// </summary>
    private void Flee(World world, Entity entity, Position position, DungeonMap map)
    {
        var playerPos = GetPlayerPosition(world);

        if (playerPos == null)
        {
            Wander(world, entity, position, map);
            return;
        }

        // Move away from player
        var direction = position.Point - playerPos.Value.Point;

        // Normalize and extend the direction
        Point fleeTarget;
        if (direction.X == 0 && direction.Y == 0)
        {
            // If on same tile, pick random direction
            var randomDirIndex = _random.Next(8);
            var dirs = new[] {
                SadRogue.Primitives.Direction.Up,
                SadRogue.Primitives.Direction.UpRight,
                SadRogue.Primitives.Direction.Right,
                SadRogue.Primitives.Direction.DownRight,
                SadRogue.Primitives.Direction.Down,
                SadRogue.Primitives.Direction.DownLeft,
                SadRogue.Primitives.Direction.Left,
                SadRogue.Primitives.Direction.UpLeft
            };
            fleeTarget = position.Point + dirs[randomDirIndex];
        }
        else
        {
            // Move in opposite direction of player
            var normalizedX = Math.Sign(direction.X);
            var normalizedY = Math.Sign(direction.Y);
            fleeTarget = position.Point + new Point(normalizedX, normalizedY);
        }

        var newPosition = new Position(fleeTarget);

        if (!_movementSystem.MoveEntity(world, entity, newPosition, map))
        {
            // If can't flee, try wandering
            Wander(world, entity, position, map);
        }
    }

    /// <summary>
    /// AI behavior: Patrol (simple back and forth for now)
    /// </summary>
    private void Patrol(World world, Entity entity, Position position, DungeonMap map)
    {
        // Simple patrol: alternate between moving in a direction
        // For now, just wander - can be enhanced with waypoints
        Wander(world, entity, position, map);
    }

    /// <summary>
    /// Gets the player's position
    /// </summary>
    private Position? GetPlayerPosition(World world)
    {
        var query = new QueryDescription().WithAll<Player, Position>();

        Position? result = null;

        world.Query(in query, (Entity entity, ref Player player, ref Position pos) =>
        {
            result = pos;
        });

        return result;
    }

    /// <summary>
    /// Gets the player entity
    /// </summary>
    private Entity? GetPlayer(World world)
    {
        var query = new QueryDescription().WithAll<Player>();

        Entity? result = null;

        world.Query(in query, (Entity entity) =>
        {
            result = entity;
        });

        return result;
    }
}
