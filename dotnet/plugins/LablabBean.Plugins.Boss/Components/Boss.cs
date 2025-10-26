namespace LablabBean.Plugins.Boss.Components;

/// <summary>
/// Core boss component with phases and abilities.
/// </summary>
public struct Boss
{
    public string Id;
    public string Name;
    public BossType Type;
    public int CurrentPhase;
    public Dictionary<int, BossPhase> Phases;
    public List<string> AbilityIds;
    public bool IsEnraged;
    public float EnrageTimer;
    public int PlayerLevel;
    public Dictionary<string, float> AbilityCooldowns;
}

/// <summary>
/// Boss difficulty tier.
/// </summary>
public enum BossType
{
    Mini,
    Standard,
    Epic,
    Raid
}

/// <summary>
/// Boss phase definition.
/// </summary>
public struct BossPhase
{
    public int PhaseNumber;
    public float HealthThreshold;
    public List<string> EnabledAbilities;
    public float DamageModifier;
    public float DefenseModifier;
    public string PhaseTransitionText;
    public bool HealOnTransition;
    public int HealAmount;
}

/// <summary>
/// Boss ability definition.
/// </summary>
public struct BossAbility
{
    public string Id;
    public string Name;
    public string Description;
    public AbilityType Type;
    public float Cooldown;
    public int Damage;
    public float Range;
    public List<string> StatusEffects;
    public Dictionary<string, object> Parameters;
}

/// <summary>
/// Boss ability types.
/// </summary>
public enum AbilityType
{
    Summon,
    AoE,
    SingleTarget,
    Buff,
    Debuff,
    Heal,
    Teleport,
    Shield,
    Transform
}

/// <summary>
/// Boss loot configuration.
/// </summary>
public struct BossLoot
{
    public string BossId;
    public List<BossLootEntry> GuaranteedLoot;
    public List<BossLootEntry> RandomLoot;
    public int MinGold;
    public int MaxGold;
    public int ExperienceReward;
}

/// <summary>
/// Individual boss loot entry.
/// </summary>
public struct BossLootEntry
{
    public string ItemId;
    public string ItemName;
    public string Rarity;
    public float DropChance;
    public int MinQuantity;
    public int MaxQuantity;
}
