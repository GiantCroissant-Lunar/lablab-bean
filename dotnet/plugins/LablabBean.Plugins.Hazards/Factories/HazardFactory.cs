using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.Hazards.Components;
using LablabBean.Plugins.Hazards.Data;
using LablabBean.Plugins.Hazards.Systems.Components;

namespace LablabBean.Plugins.Hazards.Factories;

/// <summary>
/// Factory for creating hazard entities
/// </summary>
public class HazardFactory
{
    private readonly World _world;

    public HazardFactory(World world)
    {
        _world = world;
    }

    /// <summary>
    /// Create a spike trap
    /// </summary>
    public Entity CreateSpikeTrap(int x, int y, bool hidden = true)
    {
        var hazard = new Hazard(HazardType.SpikeTrap, 10, 0.8f, !hidden, hidden, 12);
        var trigger = new HazardTrigger(TriggerType.OnEnter, false);
        var transform = new Transform { X = x, Y = y };

        return _world.Create(hazard, trigger, transform);
    }

    /// <summary>
    /// Create a bear trap
    /// </summary>
    public Entity CreateBearTrap(int x, int y, bool hidden = true)
    {
        var hazard = new Hazard(HazardType.BearTrap, 15, 1.0f, !hidden, hidden, 15);
        var trigger = new HazardTrigger(TriggerType.OnEnter, false);
        var transform = new Transform { X = x, Y = y };

        return _world.Create(hazard, trigger, transform);
    }

    /// <summary>
    /// Create an arrow trap
    /// </summary>
    public Entity CreateArrowTrap(int x, int y, bool hidden = true)
    {
        var hazard = new Hazard(HazardType.ArrowTrap, 12, 1.0f, !hidden, hidden, 14);
        var trigger = new HazardTrigger(TriggerType.Proximity, true, 3)
        {
            ProximityRange = 1
        };
        var transform = new Transform { X = x, Y = y };

        return _world.Create(hazard, trigger, transform);
    }

    /// <summary>
    /// Create lava tile
    /// </summary>
    public Entity CreateLava(int x, int y)
    {
        var hazard = new Hazard(HazardType.Lava, 20, 1.0f, true, false, 0);
        var trigger = new HazardTrigger(TriggerType.OnEnter, true);
        var transform = new Transform { X = x, Y = y };

        return _world.Create(hazard, trigger, transform);
    }

    /// <summary>
    /// Create poison gas cloud
    /// </summary>
    public Entity CreatePoisonGas(int x, int y)
    {
        var hazard = new Hazard(HazardType.PoisonGas, 5, 1.0f, true, false, 0);
        var trigger = new HazardTrigger(TriggerType.Periodic, true)
        {
            Period = 2
        };
        var transform = new Transform { X = x, Y = y };

        return _world.Create(hazard, trigger, transform);
    }

    /// <summary>
    /// Create acid pool
    /// </summary>
    public Entity CreateAcidPool(int x, int y)
    {
        var hazard = new Hazard(HazardType.AcidPool, 15, 1.0f, true, false, 0);
        var trigger = new HazardTrigger(TriggerType.OnEnter, true);
        var transform = new Transform { X = x, Y = y };

        return _world.Create(hazard, trigger, transform);
    }

    /// <summary>
    /// Create electric floor
    /// </summary>
    public Entity CreateElectricFloor(int x, int y)
    {
        var hazard = new Hazard(HazardType.ElectricFloor, 18, 1.0f, true, false, 0);
        var trigger = new HazardTrigger(TriggerType.Periodic, true)
        {
            Period = 3
        };
        var transform = new Transform { X = x, Y = y };

        return _world.Create(hazard, trigger, transform);
    }

    /// <summary>
    /// Create falling rocks trap
    /// </summary>
    public Entity CreateFallingRocks(int x, int y, bool hidden = true)
    {
        var hazard = new Hazard(HazardType.FallingRocks, 25, 0.6f, !hidden, hidden, 16);
        var trigger = new HazardTrigger(TriggerType.OnEnter, false);
        var transform = new Transform { X = x, Y = y };

        return _world.Create(hazard, trigger, transform);
    }

    /// <summary>
    /// Create pitfall trap
    /// </summary>
    public Entity CreatePitfall(int x, int y, bool hidden = true)
    {
        var hazard = new Hazard(HazardType.Pitfall, 20, 1.0f, !hidden, hidden, 13);
        var trigger = new HazardTrigger(TriggerType.OnEnter, false);
        var transform = new Transform { X = x, Y = y };

        return _world.Create(hazard, trigger, transform);
    }

    /// <summary>
    /// Create fire
    /// </summary>
    public Entity CreateFire(int x, int y)
    {
        var hazard = new Hazard(HazardType.Fire, 8, 1.0f, true, false, 0);
        var trigger = new HazardTrigger(TriggerType.OnEnter, true);
        var transform = new Transform { X = x, Y = y };

        return _world.Create(hazard, trigger, transform);
    }

    /// <summary>
    /// Create hazard from definition
    /// </summary>
    public Entity CreateFromDefinition(HazardDefinition def, int x, int y)
    {
        var hazard = new Hazard(
            def.Type,
            def.Damage,
            def.ActivationChance,
            def.IsVisible,
            def.RequiresDetection,
            def.DetectionDifficulty
        );

        var trigger = new HazardTrigger(def.TriggerType, def.CanRetrigger, def.RetriggerDelay)
        {
            Period = def.TriggerPeriod,
            ProximityRange = def.ProximityRange
        };

        var transform = new Transform { X = x, Y = y };

        return _world.Create(hazard, trigger, transform);
    }
}
