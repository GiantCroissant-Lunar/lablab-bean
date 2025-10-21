using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Game.Core.Components;
using Microsoft.Extensions.Logging;

namespace LablabBean.Game.Core.Systems;

/// <summary>
/// System that manages the actor/turn-based system
/// Accumulates energy for actors and determines turn order
/// </summary>
public class ActorSystem
{
    private readonly ILogger<ActorSystem> _logger;

    public ActorSystem(ILogger<ActorSystem> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Accumulates energy for all actors
    /// </summary>
    public void AccumulateEnergy(World world)
    {
        var query = new QueryDescription().WithAll<Actor>();

        world.Query(in query, (Entity entity, ref Actor actor) =>
        {
            actor.AccumulateEnergy();
        });
    }

    /// <summary>
    /// Gets all actors that can act (have enough energy)
    /// Returns them sorted by remaining energy (highest first)
    /// </summary>
    public List<Entity> GetActorsReadyToAct(World world)
    {
        var query = new QueryDescription().WithAll<Actor>();
        var readyActors = new List<(Entity entity, int energy)>();

        world.Query(in query, (Entity entity, ref Actor actor) =>
        {
            if (actor.CanAct)
            {
                readyActors.Add((entity, actor.Energy));
            }
        });

        // Sort by energy (highest first)
        return readyActors
            .OrderByDescending(x => x.energy)
            .Select(x => x.entity)
            .ToList();
    }

    /// <summary>
    /// Checks if any actor can act
    /// </summary>
    public bool AnyActorCanAct(World world)
    {
        var query = new QueryDescription().WithAll<Actor>();
        bool canAct = false;

        world.Query(in query, (Entity entity, ref Actor actor) =>
        {
            if (actor.CanAct)
            {
                canAct = true;
            }
        });

        return canAct;
    }

    /// <summary>
    /// Gets the next actor to take a turn
    /// </summary>
    public Entity? GetNextActor(World world)
    {
        var readyActors = GetActorsReadyToAct(world);
        return readyActors.FirstOrDefault();
    }

    /// <summary>
    /// Checks if it's the player's turn
    /// </summary>
    public bool IsPlayerTurn(World world)
    {
        var query = new QueryDescription().WithAll<Player, Actor>();
        bool isPlayerTurn = false;

        world.Query(in query, (Entity entity, ref Player player, ref Actor actor) =>
        {
            if (actor.CanAct)
            {
                isPlayerTurn = true;
            }
        });

        return isPlayerTurn;
    }

    /// <summary>
    /// Processes one game tick
    /// Accumulates energy until at least one actor can act
    /// </summary>
    public void ProcessTick(World world)
    {
        while (!AnyActorCanAct(world))
        {
            AccumulateEnergy(world);
        }
    }

    /// <summary>
    /// Process status effects for an actor at the start of their turn
    /// Returns messages for display
    /// </summary>
    public List<string> ProcessActorEffects(World world, Entity actor, StatusEffectSystem statusEffectSystem)
    {
        return statusEffectSystem.ProcessEffects(world, actor);
    }
}
