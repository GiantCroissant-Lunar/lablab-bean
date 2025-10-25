using Akka.Actor;
using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.AI.Actors.Bridges;
using LablabBean.AI.Actors.Messages;
using LablabBean.AI.Core.Components;
using LablabBean.AI.Core.Events;
using LablabBean.AI.Core.Interfaces;
using LablabBean.AI.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace LablabBean.AI.Actors.Systems;

public class IntelligentAISystem
{
    private readonly ILogger<IntelligentAISystem> _logger;
    private readonly ActorSystem _actorSystem;
    private readonly IActorRef _eventBusAdapter;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<int, IActorRef> _entityActorMap;
    private readonly Dictionary<int, AICapability> _entityCapabilityMap;

    public IntelligentAISystem(
        ILogger<IntelligentAISystem> logger,
        ActorSystem actorSystem,
        IServiceProvider serviceProvider,
        Action<object>? eventPublisher = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _actorSystem = actorSystem ?? throw new ArgumentNullException(nameof(actorSystem));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _entityActorMap = new Dictionary<int, IActorRef>();
        _entityCapabilityMap = new Dictionary<int, AICapability>();

        _eventBusAdapter = _actorSystem.ActorOf(
            EventBusAkkaAdapter.Props(eventPublisher),
            "event-bus-adapter");

        _logger.LogInformation("IntelligentAISystem initialized with ActorSystem: {ActorSystem}", _actorSystem.Name);
    }

    public void Update(World world, float deltaTime)
    {
        var query = new QueryDescription().WithAll<IntelligentAI>();
        var entitiesToProcess = new List<(int entityId, Entity entity, IntelligentAI intelligentAI)>();

        world.Query(in query, (Entity entity, ref IntelligentAI intelligentAI) =>
        {
            entitiesToProcess.Add((entity.Id, entity, intelligentAI));
        });

        foreach (var (entityId, entity, intelligentAI) in entitiesToProcess)
        {
            var ai = intelligentAI;
            if (!_entityActorMap.ContainsKey(entityId))
            {
                SpawnActorForEntity(entity, ref ai);
            }
            else
            {
                UpdateActorState(world, entity, ref ai, deltaTime);
            }
        }

        CleanupDeadActors(world);
    }

    private void SpawnActorForEntity(Entity entity, ref IntelligentAI intelligentAI)
    {
        var entityId = entity.Id;
        var capabilities = intelligentAI.Capabilities;
        try
        {
            IActorRef actorRef;
            string actorName = $"entity-{entityId}";
            if (HasBossCapabilities(capabilities))
            {
                actorRef = SpawnBossActor(entityId, actorName);
                _logger.LogInformation("Spawned BossActor for entity {EntityId}", entityId);
            }
            else if (HasEmployeeCapabilities(capabilities))
            {
                actorRef = SpawnEmployeeActor(entityId, actorName);
                _logger.LogInformation("Spawned EmployeeActor for entity {EntityId}", entityId);
            }
            else
            {
                _logger.LogWarning("Entity {EntityId} has IntelligentAI but no recognized capabilities", entityId);
                return;
            }
            _entityActorMap[entityId] = actorRef;
            _entityCapabilityMap[entityId] = capabilities;
            entity.Add(new AkkaActorRef(actorRef, actorRef.Path.ToString()));
            _logger.LogDebug("Linked actor to entity {EntityId}", entityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to spawn actor for entity {EntityId}", entityId);
        }
    }

    private IActorRef SpawnBossActor(int entityId, string actorName)
    {
        var personality = CreateDefaultBossPersonality();
        var agent = _serviceProvider.GetService<IIntelligenceAgent>();
        var adapter = _eventBusAdapter;
        var entityIdString = entityId.ToString();

        return _actorSystem.ActorOf(
            Props.Create(typeof(BossActor), entityIdString, personality, adapter, agent, 10),
            actorName);
    }

    private IActorRef SpawnEmployeeActor(int entityId, string actorName)
    {
        var personality = CreateDefaultEmployeePersonality();
        var agent = _serviceProvider.GetService<IIntelligenceAgent>();
        var adapter = _eventBusAdapter;
        var entityIdString = entityId.ToString();

        return _actorSystem.ActorOf(
            Props.Create(typeof(EmployeeActor), entityIdString, personality, adapter, agent, 10),
            actorName);
    }

    private void UpdateActorState(World world, Entity entity, ref IntelligentAI intelligentAI, float deltaTime)
    {
        var entityId = entity.Id;
        if (!_entityActorMap.TryGetValue(entityId, out var actorRef)) return;
        intelligentAI.TimeSinceLastDecision += deltaTime;
        if (intelligentAI.TimeSinceLastDecision < intelligentAI.DecisionCooldown) return;
        intelligentAI.TimeSinceLastDecision = 0f;
        try
        {
            if (entity.Has<LablabBean.Game.Core.Components.Position>())
            {
                var query = new QueryDescription().WithAll<LablabBean.Game.Core.Components.Player, LablabBean.Game.Core.Components.Position>();
                Entity? playerEntity = null;
                world.Query(in query, (Entity e) => { playerEntity = e; });

                if (playerEntity.HasValue && playerEntity.Value.Has<LablabBean.Game.Core.Components.Position>())
                {
                    var entityPos = entity.Get<LablabBean.Game.Core.Components.Position>();
                    var playerPos = playerEntity.Value.Get<LablabBean.Game.Core.Components.Position>();
                    var distance = CalculateDistance(entityPos.Point, playerPos.Point);
                    if (distance <= 10.0f)
                    {
                        actorRef.Tell(new PlayerNearbyMessage(playerEntity.Value.Id.ToString(), distance, DateTime.UtcNow));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating actor state for entity {EntityId}", entityId);
        }
    }

    private void CleanupDeadActors(World world)
    {
        var deadEntities = new List<int>();
        foreach (var (entityId, actorRef) in _entityActorMap)
        {
            try
            {
                bool entityExists = false;
                var query = new QueryDescription().WithAll<IntelligentAI>();
                world.Query(in query, (Entity e, ref IntelligentAI ai) =>
                {
                    if (e.Id == entityId)
                    {
                        entityExists = true;
                    }
                });

                if (!entityExists)
                {
                    deadEntities.Add(entityId);
                    _actorSystem.Stop(actorRef);
                    _eventBusAdapter.Tell(new ActorStoppedEvent
                    {
                        EntityId = entityId.ToString(),
                        ActorPath = actorRef.Path.ToString(),
                        Reason = "Entity destroyed or lost IntelligentAI component",
                        Timestamp = DateTime.UtcNow
                    });
                    _logger.LogInformation("Stopped actor for entity {EntityId}", entityId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking entity {EntityId}, marking for cleanup", entityId);
                deadEntities.Add(entityId);
            }
        }
        foreach (var entityId in deadEntities)
        {
            _entityActorMap.Remove(entityId);
            _entityCapabilityMap.Remove(entityId);
        }
    }

    public void SendMessageToEntity(int entityId, object message)
    {
        if (_entityActorMap.TryGetValue(entityId, out var actorRef))
        {
            actorRef.Tell(message);
        }
    }

    public void BroadcastMessage(object message)
    {
        foreach (var actorRef in _entityActorMap.Values) actorRef.Tell(message);
    }

    private static bool HasBossCapabilities(AICapability capabilities) =>
        capabilities.HasFlag(AICapability.TacticalAdaptation) && capabilities.HasFlag(AICapability.QuestGeneration);

    private static bool HasEmployeeCapabilities(AICapability capabilities) =>
        capabilities.HasFlag(AICapability.Dialogue) && capabilities.HasFlag(AICapability.Memory);

    private static BossPersonality CreateDefaultBossPersonality() => new BossPersonality
    {
        Name = "Default Boss",
        Version = "1.0.0",
        AvatarType = "boss"
    };

    private static EmployeePersonality CreateDefaultEmployeePersonality() => new EmployeePersonality
    {
        Name = "Default Employee",
        Version = "1.0.0",
        AvatarType = "employee"
    };

    private static float CalculateDistance(SadRogue.Primitives.Point p1, SadRogue.Primitives.Point p2) =>
        Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);

    public void Shutdown()
    {
        _logger.LogInformation("Shutting down IntelligentAISystem...");
        foreach (var (entityId, actorRef) in _entityActorMap)
        {
            try { _actorSystem.Stop(actorRef); }
            catch (Exception ex) { _logger.LogError(ex, "Error stopping actor for entity {EntityId}", entityId); }
        }
        _entityActorMap.Clear();
        _entityCapabilityMap.Clear();
        _logger.LogInformation("IntelligentAISystem shutdown complete");
    }
}
