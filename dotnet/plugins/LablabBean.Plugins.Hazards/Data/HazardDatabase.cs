using LablabBean.Plugins.Hazards.Components;

namespace LablabBean.Plugins.Hazards.Data;

/// <summary>
/// Database of predefined hazards
/// </summary>
public static class HazardDatabase
{
    public static readonly Dictionary<string, HazardDefinition> Hazards = new()
    {
        ["spike_trap"] = new HazardDefinition
        {
            Id = "spike_trap",
            Name = "Spike Trap",
            Description = "Sharp spikes spring from the ground when stepped on",
            Type = HazardType.SpikeTrap,
            Damage = 10,
            ActivationChance = 0.8f,
            IsVisible = false,
            RequiresDetection = true,
            DetectionDifficulty = 12,
            TriggerType = TriggerType.OnEnter,
            CanRetrigger = false,
            Glyph = '^',
            Color = "Gray"
        },

        ["bear_trap"] = new HazardDefinition
        {
            Id = "bear_trap",
            Name = "Bear Trap",
            Description = "A metal trap that clamps down on victims",
            Type = HazardType.BearTrap,
            Damage = 15,
            ActivationChance = 1.0f,
            IsVisible = false,
            RequiresDetection = true,
            DetectionDifficulty = 15,
            TriggerType = TriggerType.OnEnter,
            CanRetrigger = false,
            Glyph = 'v',
            Color = "DarkGray"
        },

        ["arrow_trap"] = new HazardDefinition
        {
            Id = "arrow_trap",
            Name = "Arrow Trap",
            Description = "Fires arrows when triggered",
            Type = HazardType.ArrowTrap,
            Damage = 12,
            ActivationChance = 1.0f,
            IsVisible = false,
            RequiresDetection = true,
            DetectionDifficulty = 14,
            TriggerType = TriggerType.Proximity,
            ProximityRange = 1,
            CanRetrigger = true,
            RetriggerDelay = 3,
            Glyph = '→',
            Color = "Brown"
        },

        ["lava"] = new HazardDefinition
        {
            Id = "lava",
            Name = "Lava",
            Description = "Molten rock that burns everything it touches",
            Type = HazardType.Lava,
            Damage = 20,
            ActivationChance = 1.0f,
            IsVisible = true,
            RequiresDetection = false,
            TriggerType = TriggerType.OnEnter,
            CanRetrigger = true,
            Glyph = '≈',
            Color = "Red"
        },

        ["poison_gas"] = new HazardDefinition
        {
            Id = "poison_gas",
            Name = "Poison Gas",
            Description = "Toxic gas that poisons those who breathe it",
            Type = HazardType.PoisonGas,
            Damage = 5,
            ActivationChance = 1.0f,
            IsVisible = true,
            RequiresDetection = false,
            TriggerType = TriggerType.Periodic,
            TriggerPeriod = 2,
            CanRetrigger = true,
            Glyph = '☁',
            Color = "Green"
        },

        ["acid_pool"] = new HazardDefinition
        {
            Id = "acid_pool",
            Name = "Acid Pool",
            Description = "Corrosive acid that eats through armor",
            Type = HazardType.AcidPool,
            Damage = 15,
            ActivationChance = 1.0f,
            IsVisible = true,
            RequiresDetection = false,
            TriggerType = TriggerType.OnEnter,
            CanRetrigger = true,
            Glyph = '~',
            Color = "Yellow"
        },

        ["electric_floor"] = new HazardDefinition
        {
            Id = "electric_floor",
            Name = "Electric Floor",
            Description = "Electrified floor that shocks on contact",
            Type = HazardType.ElectricFloor,
            Damage = 18,
            ActivationChance = 1.0f,
            IsVisible = true,
            RequiresDetection = false,
            TriggerType = TriggerType.Periodic,
            TriggerPeriod = 3,
            CanRetrigger = true,
            Glyph = '▓',
            Color = "Cyan"
        },

        ["falling_rocks"] = new HazardDefinition
        {
            Id = "falling_rocks",
            Name = "Falling Rocks",
            Description = "Ceiling collapses when walked under",
            Type = HazardType.FallingRocks,
            Damage = 25,
            ActivationChance = 0.6f,
            IsVisible = false,
            RequiresDetection = true,
            DetectionDifficulty = 16,
            TriggerType = TriggerType.OnEnter,
            CanRetrigger = false,
            Glyph = '▼',
            Color = "Gray"
        },

        ["pitfall"] = new HazardDefinition
        {
            Id = "pitfall",
            Name = "Pitfall",
            Description = "Hidden pit that causes falling damage",
            Type = HazardType.Pitfall,
            Damage = 20,
            ActivationChance = 1.0f,
            IsVisible = false,
            RequiresDetection = true,
            DetectionDifficulty = 13,
            TriggerType = TriggerType.OnEnter,
            CanRetrigger = false,
            Glyph = 'O',
            Color = "Black"
        },

        ["fire"] = new HazardDefinition
        {
            Id = "fire",
            Name = "Fire",
            Description = "Burning flames that spread heat damage",
            Type = HazardType.Fire,
            Damage = 8,
            ActivationChance = 1.0f,
            IsVisible = true,
            RequiresDetection = false,
            TriggerType = TriggerType.OnEnter,
            CanRetrigger = true,
            Glyph = '♦',
            Color = "Red"
        }
    };

    public static HazardDefinition? GetHazard(string id)
    {
        return Hazards.TryGetValue(id, out var hazard) ? hazard : null;
    }

    public static List<HazardDefinition> GetHazardsByType(HazardType type)
    {
        return Hazards.Values.Where(h => h.Type == type).ToList();
    }

    public static List<HazardDefinition> GetAllHazards()
    {
        return Hazards.Values.ToList();
    }
}
