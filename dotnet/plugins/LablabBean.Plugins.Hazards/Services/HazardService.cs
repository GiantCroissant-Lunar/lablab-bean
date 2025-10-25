using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.Hazards.Components;
using LablabBean.Plugins.Hazards.Data;
using LablabBean.Plugins.Hazards.Systems;
using LablabBean.Plugins.Hazards.Systems.Components;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Hazards.Services;

/// <summary>
/// High-level service for managing hazards
/// </summary>
public class HazardService
{
    private readonly World _world;
    private readonly HazardSystem _hazardSystem;
    private readonly HazardDetectionSystem _detectionSystem;
    private readonly ILogger<HazardService> _logger;

    public HazardService(
        World world,
        HazardSystem hazardSystem,
        HazardDetectionSystem detectionSystem,
        ILogger<HazardService> logger)
    {
        _world = world;
        _hazardSystem = hazardSystem;
        _detectionSystem = detectionSystem;
        _logger = logger;
    }

    /// <summary>
    /// Create a hazard from a definition
    /// </summary>
    public Entity CreateHazard(string hazardId, int x, int y)
    {
        var definition = HazardDatabase.GetHazard(hazardId);
        if (definition == null)
        {
            _logger.LogError($"Hazard definition not found: {hazardId}");
            throw new ArgumentException($"Unknown hazard: {hazardId}");
        }

        return CreateHazardFromDefinition(definition, x, y);
    }

    /// <summary>
    /// Create a hazard entity from definition
    /// </summary>
    public Entity CreateHazardFromDefinition(HazardDefinition definition, int x, int y)
    {
        var hazard = new Hazard(
            definition.Type,
            definition.Damage,
            definition.ActivationChance,
            definition.IsVisible,
            definition.RequiresDetection,
            definition.DetectionDifficulty
        );

        var trigger = new HazardTrigger(definition.TriggerType, definition.CanRetrigger, definition.RetriggerDelay)
        {
            Period = definition.TriggerPeriod,
            ProximityRange = definition.ProximityRange
        };

        var transform = new Transform { X = x, Y = y };

        var entity = _world.Create(hazard, trigger, transform);

        _logger.LogInformation($"Created {definition.Name} at ({x}, {y})");
        return entity;
    }

    /// <summary>
    /// Process all hazards (called each game turn)
    /// </summary>
    public void UpdateHazards()
    {
        _hazardSystem.ProcessHazards();
    }

    /// <summary>
    /// Check for hazards when entity moves
    /// </summary>
    public void OnEntityMove(Entity entity, int x, int y)
    {
        _hazardSystem.CheckHazardActivation(x, y, entity);
        _hazardSystem.CheckProximityHazards(x, y, entity);
    }

    /// <summary>
    /// Attempt to detect hazards in area
    /// </summary>
    public List<Entity> DetectHazards(int x, int y, int range, int skill)
    {
        return _detectionSystem.DetectHazards(x, y, range, skill);
    }

    /// <summary>
    /// Attempt to disarm a hazard
    /// </summary>
    public bool DisarmHazard(Entity hazardEntity, int skill)
    {
        return _detectionSystem.DisarmHazard(hazardEntity, skill);
    }

    /// <summary>
    /// Get hazards at a specific position
    /// </summary>
    public List<Entity> GetHazardsAtPosition(int x, int y)
    {
        var hazards = new List<Entity>();
        var query = new QueryDescription().WithAll<Hazard, Transform>();

        _world.Query(in query, (Entity entity, ref Hazard hazard, ref Transform transform) =>
        {
            if (transform.X == x && transform.Y == y)
            {
                hazards.Add(entity);
            }
        });

        return hazards;
    }

    /// <summary>
    /// Get information about a hazard
    /// </summary>
    public string GetHazardInfo(Entity hazardEntity)
    {
        return _detectionSystem.GetHazardInfo(hazardEntity);
    }

    /// <summary>
    /// Add hazard resistance to an entity
    /// </summary>
    public void AddResistance(Entity entity, HazardType hazardType, float amount)
    {
        HazardResistance resistance;

        if (entity.Has<HazardResistance>())
        {
            resistance = entity.Get<HazardResistance>();
        }
        else
        {
            resistance = new HazardResistance();
        }

        resistance.SetResistance(hazardType, amount);
        entity.Set(in resistance);

        _logger.LogDebug($"Added {amount * 100}% resistance to {hazardType} for entity {entity.Id}");
    }

    /// <summary>
    /// Remove hazard effect from entity
    /// </summary>
    public void RemoveHazardEffect(Entity entity)
    {
        if (entity.Has<HazardEffect>())
        {
            entity.Remove<HazardEffect>();
            _logger.LogDebug($"Removed hazard effect from entity {entity.Id}");
        }
    }

    /// <summary>
    /// Get all active hazards in the world
    /// </summary>
    public List<(Entity entity, Hazard hazard)> GetAllHazards()
    {
        var hazards = new List<(Entity, Hazard)>();
        var query = new QueryDescription().WithAll<Hazard>();

        _world.Query(in query, (Entity entity, ref Hazard hazard) =>
        {
            hazards.Add((entity, hazard));
        });

        return hazards;
    }

    /// <summary>
    /// Spawn hazards in a room/area
    /// </summary>
    public void SpawnHazardsInArea(int x, int y, int width, int height, int hazardCount)
    {
        var random = new Random();
        var hazardTypes = HazardDatabase.GetAllHazards();

        for (int i = 0; i < hazardCount; i++)
        {
            var hx = random.Next(x, x + width);
            var hy = random.Next(y, y + height);
            var hazardDef = hazardTypes[random.Next(hazardTypes.Count)];

            CreateHazardFromDefinition(hazardDef, hx, hy);
        }

        _logger.LogInformation($"Spawned {hazardCount} hazards in area ({x},{y}) to ({x + width},{y + height})");
    }
}
