using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.Hazards.Components;
using LablabBean.Plugins.Hazards.Systems.Components;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Hazards.Systems;

/// <summary>
/// Handles hazard detection and disabling
/// </summary>
public class HazardDetectionSystem
{
    private readonly World _world;
    private readonly ILogger<HazardDetectionSystem> _logger;
    private readonly Random _random = new();

    public HazardDetectionSystem(World world, ILogger<HazardDetectionSystem> logger)
    {
        _world = world;
        _logger = logger;
    }

    /// <summary>
    /// Attempt to detect hidden hazards in an area
    /// </summary>
    /// <returns>List of detected hazard entities</returns>
    public List<Entity> DetectHazards(int x, int y, int range, int detectionSkill)
    {
        var detected = new List<Entity>();
        var query = new QueryDescription().WithAll<Hazard, Transform>();

        _world.Query(in query, (Entity hazardEntity, ref Hazard hazard, ref Transform hazardTransform) =>
        {
            if (!hazard.RequiresDetection || hazard.IsVisible)
                return;

            var distance = Math.Abs(hazardTransform.X - x) + Math.Abs(hazardTransform.Y - y);
            if (distance > range)
                return;

            // Detection check: skill + d20 vs difficulty
            var roll = _random.Next(1, 21);
            var total = detectionSkill + roll;

            if (total >= hazard.DetectionDifficulty)
            {
                var newHazard = hazard;
                newHazard.IsVisible = true;
                hazardEntity.Set(in newHazard);
                detected.Add(hazardEntity);
                _logger.LogInformation($"Detected {hazard.Type} hazard at position");
            }
        });

        return detected;
    }

    /// <summary>
    /// Attempt to disarm a hazard
    /// </summary>
    public bool DisarmHazard(Entity hazardEntity, int disarmSkill)
    {
        if (!hazardEntity.Has<Hazard>())
            return false;

        var hazard = hazardEntity.Get<Hazard>();

        if (hazard.State == HazardState.Disabled)
        {
            _logger.LogWarning("Hazard is already disabled");
            return false;
        }

        // Disarm check: skill + d20 vs (difficulty + 5)
        var roll = _random.Next(1, 21);
        var total = disarmSkill + roll;
        var difficulty = hazard.DetectionDifficulty + 5;

        if (total >= difficulty)
        {
            hazard.State = HazardState.Disabled;
            hazardEntity.Set(in hazard);
            _logger.LogInformation($"Successfully disarmed {hazard.Type} hazard");
            return true;
        }

        // Critical failure: trigger the trap
        if (roll == 1)
        {
            _logger.LogWarning($"Critical failure disarming {hazard.Type}! Hazard triggered.");
            hazard.State = HazardState.Triggered;
            hazardEntity.Set(in hazard);
        }

        return false;
    }

    /// <summary>
    /// Manually activate a hazard
    /// </summary>
    public void ActivateHazard(Entity hazardEntity)
    {
        if (!hazardEntity.Has<Hazard>())
            return;

        var hazard = hazardEntity.Get<Hazard>();

        if (hazard.State == HazardState.Disabled)
        {
            _logger.LogWarning("Cannot activate disabled hazard");
            return;
        }

        hazard.State = HazardState.Active;
        hazardEntity.Set(in hazard);
        _logger.LogInformation($"Activated {hazard.Type} hazard");
    }

    /// <summary>
    /// Get information about a hazard for UI display
    /// </summary>
    public string GetHazardInfo(Entity hazardEntity)
    {
        if (!hazardEntity.Has<Hazard>())
            return "Unknown hazard";

        var hazard = hazardEntity.Get<Hazard>();
        var info = $"{hazard.Type} ({hazard.State})";

        if (hazard.IsVisible)
        {
            info += $"\nDamage: {hazard.Damage}";
            info += $"\nActivation: {hazard.ActivationChance * 100}%";
        }

        return info;
    }
}
