using System.Text.Json;
using BossComponent = LablabBean.Plugins.Boss.Components.Boss;
using LablabBean.Plugins.Boss.Components;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Boss.Services;

/// <summary>
/// Implementation of boss service with data-driven boss definitions.
/// </summary>
public class BossService : IBossService
{
    private readonly ILogger<BossService> _logger;
    private readonly Dictionary<string, BossDefinition> _bossDatabase;
    private readonly Dictionary<string, List<BossAbility>> _abilityDatabase;
    private readonly Dictionary<string, BossLoot> _lootDatabase;
    private readonly Random _random = new();

    public BossService(ILogger<BossService> logger)
    {
        _logger = logger;
        _bossDatabase = LoadBossDatabase();
        _abilityDatabase = LoadAbilityDatabase();
        _lootDatabase = LoadLootDatabase();
    }

    public BossComponent CreateBoss(string bossId, int playerLevel)
    {
        if (!_bossDatabase.TryGetValue(bossId, out var definition))
        {
            _logger.LogError("Boss {BossId} not found in database", bossId);
            throw new KeyNotFoundException($"Boss {bossId} not found");
        }

        var boss = new BossComponent
        {
            Id = bossId,
            Name = definition.Name,
            Type = definition.Type,
            CurrentPhase = 1,
            Phases = definition.Phases.ToDictionary(p => p.PhaseNumber, p => p),
            AbilityIds = definition.AbilityIds,
            IsEnraged = false,
            EnrageTimer = 0f,
            PlayerLevel = playerLevel,
            AbilityCooldowns = new Dictionary<string, float>()
        };

        ScaleBossToLevel(ref boss, playerLevel);

        _logger.LogInformation("Created boss {Name} (Level {Level})", boss.Name, playerLevel);
        return boss;
    }

    public List<BossAbility> GetBossAbilities(string bossId)
    {
        return _abilityDatabase.TryGetValue(bossId, out var abilities)
            ? abilities
            : new List<BossAbility>();
    }

    public void TriggerPhaseTransition(ref BossComponent boss, int newPhase)
    {
        if (!boss.Phases.TryGetValue(newPhase, out var phase))
        {
            _logger.LogWarning("Phase {Phase} not found for boss {Boss}", newPhase, boss.Name);
            return;
        }

        boss.CurrentPhase = newPhase;

        if (phase.HealOnTransition)
        {
            _logger.LogInformation("{Boss} heals {Amount} HP during phase transition",
                boss.Name, phase.HealAmount);
        }

        _logger.LogInformation("{Boss} enters Phase {Phase}: {Text}",
            boss.Name, newPhase, phase.PhaseTransitionText);
    }

    public bool CanUseAbility(ref BossComponent boss, string abilityId)
    {
        if (!boss.AbilityCooldowns.TryGetValue(abilityId, out var cooldown))
            return true;

        return cooldown <= 0f;
    }

    public BossAbility? SelectNextAbility(ref BossComponent boss, float currentHealthPercent)
    {
        var currentPhase = boss.Phases[boss.CurrentPhase];
        var bossId = boss.Id;
        var cooldowns = boss.AbilityCooldowns;
        var isEnraged = boss.IsEnraged;

        var availableAbilities = GetBossAbilities(bossId)
            .Where(a => currentPhase.EnabledAbilities.Contains(a.Id))
            .Where(a => !cooldowns.ContainsKey(a.Id) || cooldowns[a.Id] <= 0f)
            .OrderByDescending(a => GetAbilityPriority(a, currentHealthPercent, isEnraged))
            .ToList();

        if (availableAbilities.Count == 0)
            return null;

        return availableAbilities.FirstOrDefault();
    }

    public void UpdateCooldowns(ref BossComponent boss, float deltaTime)
    {
        var keys = boss.AbilityCooldowns.Keys.ToList();
        foreach (var key in keys)
        {
            boss.AbilityCooldowns[key] = Math.Max(0f, boss.AbilityCooldowns[key] - deltaTime);
        }

        boss.EnrageTimer += deltaTime;
    }

    public void CheckEnrage(ref BossComponent boss, float currentHealthPercent)
    {
        if (boss.IsEnraged)
            return;

        var enrageThreshold = 0.2f;
        var enrageTimeSeconds = 60f;

        if (currentHealthPercent <= enrageThreshold && boss.EnrageTimer >= enrageTimeSeconds)
        {
            boss.IsEnraged = true;
            _logger.LogWarning("{Boss} is ENRAGED!", boss.Name);
        }
    }

    public BossLoot GetBossLoot(string bossId)
    {
        return _lootDatabase.TryGetValue(bossId, out var loot)
            ? loot
            : new BossLoot { BossId = bossId, GuaranteedLoot = new(), RandomLoot = new() };
    }

    public void ScaleBossToLevel(ref BossComponent boss, int playerLevel)
    {
        var scalingMultiplier = 1.0f + (playerLevel - 1) * 0.1f;
        _logger.LogDebug("Scaling boss {Boss} with multiplier {Multiplier}",
            boss.Name, scalingMultiplier);
    }

    private int GetAbilityPriority(BossAbility ability, float healthPercent, bool isEnraged)
    {
        return ability.Type switch
        {
            AbilityType.Heal when healthPercent < 0.3f => 100,
            AbilityType.Summon when healthPercent < 0.5f => 80,
            AbilityType.AoE when isEnraged => 90,
            AbilityType.Buff when !isEnraged => 70,
            AbilityType.SingleTarget => 50,
            _ => 40
        };
    }

    private Dictionary<string, BossDefinition> LoadBossDatabase()
    {
        return new Dictionary<string, BossDefinition>
        {
            ["goblin_king"] = new()
            {
                Id = "goblin_king",
                Name = "Goblin King",
                Type = BossType.Mini,
                BaseHP = 500,
                BaseDamage = 25,
                AbilityIds = new() { "goblin_rally", "war_cry", "cleave" },
                Phases = new()
                {
                    new() { PhaseNumber = 1, HealthThreshold = 1.0f, EnabledAbilities = new() { "war_cry", "cleave" }, DamageModifier = 1.0f, DefenseModifier = 1.0f, PhaseTransitionText = "The Goblin King roars in fury!" },
                    new() { PhaseNumber = 2, HealthThreshold = 0.5f, EnabledAbilities = new() { "goblin_rally", "war_cry", "cleave" }, DamageModifier = 1.2f, DefenseModifier = 0.9f, PhaseTransitionText = "The Goblin King calls for reinforcements!" }
                }
            },
            ["corrupted_treant"] = new()
            {
                Id = "corrupted_treant",
                Name = "Corrupted Treant",
                Type = BossType.Standard,
                BaseHP = 800,
                BaseDamage = 30,
                AbilityIds = new() { "root_tangle", "poison_spores", "natures_wrath", "regeneration" },
                Phases = new()
                {
                    new() { PhaseNumber = 1, HealthThreshold = 1.0f, EnabledAbilities = new() { "root_tangle", "poison_spores" }, DamageModifier = 1.0f, DefenseModifier = 1.2f, PhaseTransitionText = "The treant's bark begins to crack..." },
                    new() { PhaseNumber = 2, HealthThreshold = 0.66f, EnabledAbilities = new() { "root_tangle", "poison_spores", "natures_wrath" }, DamageModifier = 1.1f, DefenseModifier = 1.0f, PhaseTransitionText = "Toxic sap oozes from the treant's wounds!" },
                    new() { PhaseNumber = 3, HealthThreshold = 0.33f, EnabledAbilities = new() { "poison_spores", "natures_wrath", "regeneration" }, DamageModifier = 1.3f, DefenseModifier = 0.8f, PhaseTransitionText = "The treant draws upon nature's power!", HealOnTransition = true, HealAmount = 100 }
                }
            },
            ["flame_warden"] = new()
            {
                Id = "flame_warden",
                Name = "Flame Warden",
                Type = BossType.Standard,
                BaseHP = 1200,
                BaseDamage = 40,
                AbilityIds = new() { "fireball_barrage", "flame_wall", "immolation_aura", "phoenix_form" },
                Phases = new()
                {
                    new() { PhaseNumber = 1, HealthThreshold = 1.0f, EnabledAbilities = new() { "fireball_barrage", "flame_wall" }, DamageModifier = 1.0f, DefenseModifier = 1.0f, PhaseTransitionText = "Flames dance around the Warden..." },
                    new() { PhaseNumber = 2, HealthThreshold = 0.6f, EnabledAbilities = new() { "fireball_barrage", "flame_wall", "immolation_aura" }, DamageModifier = 1.2f, DefenseModifier = 1.0f, PhaseTransitionText = "The Warden's flames intensify!" },
                    new() { PhaseNumber = 3, HealthThreshold = 0.3f, EnabledAbilities = new() { "fireball_barrage", "immolation_aura", "phoenix_form" }, DamageModifier = 1.5f, DefenseModifier = 0.7f, PhaseTransitionText = "The Warden erupts in a pillar of fire!" }
                }
            },
            ["shadow_assassin"] = new()
            {
                Id = "shadow_assassin",
                Name = "Shadow Assassin",
                Type = BossType.Standard,
                BaseHP = 1000,
                BaseDamage = 50,
                AbilityIds = new() { "shadow_step", "smoke_bomb", "poison_blade", "clone" },
                Phases = new()
                {
                    new() { PhaseNumber = 1, HealthThreshold = 1.0f, EnabledAbilities = new() { "shadow_step", "smoke_bomb", "poison_blade" }, DamageModifier = 1.0f, DefenseModifier = 0.8f, PhaseTransitionText = "The assassin vanishes into the shadows..." },
                    new() { PhaseNumber = 2, HealthThreshold = 0.4f, EnabledAbilities = new() { "shadow_step", "smoke_bomb", "poison_blade", "clone" }, DamageModifier = 1.3f, DefenseModifier = 0.6f, PhaseTransitionText = "Shadow clones emerge from the darkness!" }
                }
            },
            ["ancient_dragon"] = new()
            {
                Id = "ancient_dragon",
                Name = "Ancient Dragon",
                Type = BossType.Epic,
                BaseHP = 3000,
                BaseDamage = 70,
                AbilityIds = new() { "dragon_breath", "wing_buffet", "tail_swipe", "aerial_assault", "summon_drakes", "enrage" },
                Phases = new()
                {
                    new() { PhaseNumber = 1, HealthThreshold = 1.0f, EnabledAbilities = new() { "dragon_breath", "wing_buffet", "tail_swipe" }, DamageModifier = 1.0f, DefenseModifier = 1.5f, PhaseTransitionText = "The dragon awakens from its slumber..." },
                    new() { PhaseNumber = 2, HealthThreshold = 0.75f, EnabledAbilities = new() { "dragon_breath", "wing_buffet", "tail_swipe", "aerial_assault" }, DamageModifier = 1.2f, DefenseModifier = 1.3f, PhaseTransitionText = "The dragon takes flight!" },
                    new() { PhaseNumber = 3, HealthThreshold = 0.5f, EnabledAbilities = new() { "dragon_breath", "aerial_assault", "summon_drakes", "tail_swipe" }, DamageModifier = 1.4f, DefenseModifier = 1.0f, PhaseTransitionText = "The dragon summons its kin!" },
                    new() { PhaseNumber = 4, HealthThreshold = 0.25f, EnabledAbilities = new() { "dragon_breath", "wing_buffet", "tail_swipe", "aerial_assault", "enrage" }, DamageModifier = 1.8f, DefenseModifier = 0.8f, PhaseTransitionText = "The dragon enters a killing frenzy!" }
                }
            }
        };
    }

    private Dictionary<string, List<BossAbility>> LoadAbilityDatabase()
    {
        return new Dictionary<string, List<BossAbility>>
        {
            ["goblin_king"] = new()
            {
                new() { Id = "goblin_rally", Name = "Goblin Rally", Description = "Summons 3 goblin warriors", Type = AbilityType.Summon, Cooldown = 30f, Damage = 0, Range = 0f, StatusEffects = new(), Parameters = new() { ["summonCount"] = 3, ["summonType"] = "goblin_warrior" } },
                new() { Id = "war_cry", Name = "War Cry", Description = "+20% attack to all goblins", Type = AbilityType.Buff, Cooldown = 20f, Damage = 0, Range = 10f, StatusEffects = new() { "attack_boost" }, Parameters = new() { ["damageBonus"] = 0.2f, ["duration"] = 15f } },
                new() { Id = "cleave", Name = "Cleave", Description = "30 damage AoE attack", Type = AbilityType.AoE, Cooldown = 10f, Damage = 30, Range = 3f, StatusEffects = new(), Parameters = new() }
            },
            ["corrupted_treant"] = new()
            {
                new() { Id = "root_tangle", Name = "Root Tangle", Description = "Immobilizes target for 2 turns", Type = AbilityType.Debuff, Cooldown = 15f, Damage = 10, Range = 5f, StatusEffects = new() { "immobilized" }, Parameters = new() { ["duration"] = 2 } },
                new() { Id = "poison_spores", Name = "Poison Spores", Description = "10 damage/turn AoE poison", Type = AbilityType.AoE, Cooldown = 12f, Damage = 10, Range = 4f, StatusEffects = new() { "poison" }, Parameters = new() { ["damagePerTurn"] = 10, ["duration"] = 5 } },
                new() { Id = "natures_wrath", Name = "Nature's Wrath", Description = "50 damage + knockback", Type = AbilityType.SingleTarget, Cooldown = 18f, Damage = 50, Range = 6f, StatusEffects = new() { "knockback" }, Parameters = new() { ["knockbackDistance"] = 3 } },
                new() { Id = "regeneration", Name = "Regeneration", Description = "Heals 50 HP", Type = AbilityType.Heal, Cooldown = 25f, Damage = 0, Range = 0f, StatusEffects = new(), Parameters = new() { ["healAmount"] = 50 } }
            },
            ["flame_warden"] = new()
            {
                new() { Id = "fireball_barrage", Name = "Fireball Barrage", Description = "40 damage x3 fireballs", Type = AbilityType.SingleTarget, Cooldown = 8f, Damage = 40, Range = 8f, StatusEffects = new() { "burn" }, Parameters = new() { ["projectileCount"] = 3 } },
                new() { Id = "flame_wall", Name = "Flame Wall", Description = "Creates fire hazards", Type = AbilityType.AoE, Cooldown = 20f, Damage = 20, Range = 6f, StatusEffects = new(), Parameters = new() { ["wallLength"] = 5, ["duration"] = 10f } },
                new() { Id = "immolation_aura", Name = "Immolation Aura", Description = "15 damage/turn to nearby enemies", Type = AbilityType.AoE, Cooldown = 15f, Damage = 15, Range = 3f, StatusEffects = new() { "burn" }, Parameters = new() { ["damagePerTurn"] = 15 } },
                new() { Id = "phoenix_form", Name = "Phoenix Form", Description = "Full heal once at critical HP", Type = AbilityType.Heal, Cooldown = 999f, Damage = 0, Range = 0f, StatusEffects = new(), Parameters = new() { ["healPercent"] = 1.0f } }
            },
            ["shadow_assassin"] = new()
            {
                new() { Id = "shadow_step", Name = "Shadow Step", Description = "Teleport + 60 damage backstab", Type = AbilityType.Teleport, Cooldown = 10f, Damage = 60, Range = 8f, StatusEffects = new(), Parameters = new() { ["teleportBehind"] = true } },
                new() { Id = "smoke_bomb", Name = "Smoke Bomb", Description = "Invisibility for 2 turns", Type = AbilityType.Buff, Cooldown = 18f, Damage = 0, Range = 0f, StatusEffects = new() { "invisible" }, Parameters = new() { ["duration"] = 2 } },
                new() { Id = "poison_blade", Name = "Poison Blade", Description = "30 damage + 5 damage/turn", Type = AbilityType.SingleTarget, Cooldown = 6f, Damage = 30, Range = 2f, StatusEffects = new() { "poison" }, Parameters = new() { ["damagePerTurn"] = 5, ["duration"] = 5 } },
                new() { Id = "clone", Name = "Shadow Clone", Description = "Creates 2 shadow copies", Type = AbilityType.Summon, Cooldown = 30f, Damage = 0, Range = 0f, StatusEffects = new(), Parameters = new() { ["cloneCount"] = 2, ["cloneHP"] = 0.3f } }
            },
            ["ancient_dragon"] = new()
            {
                new() { Id = "dragon_breath", Name = "Dragon's Breath", Description = "100 damage cone attack", Type = AbilityType.AoE, Cooldown = 12f, Damage = 100, Range = 10f, StatusEffects = new() { "burn" }, Parameters = new() { ["coneAngle"] = 60 } },
                new() { Id = "wing_buffet", Name = "Wing Buffet", Description = "40 damage AoE + knockback", Type = AbilityType.AoE, Cooldown = 15f, Damage = 40, Range = 5f, StatusEffects = new() { "knockback" }, Parameters = new() { ["knockbackDistance"] = 5 } },
                new() { Id = "tail_swipe", Name = "Tail Swipe", Description = "70 damage cleave attack", Type = AbilityType.AoE, Cooldown = 10f, Damage = 70, Range = 4f, StatusEffects = new(), Parameters = new() },
                new() { Id = "aerial_assault", Name = "Aerial Assault", Description = "Flies up, immune for 1 turn", Type = AbilityType.Buff, Cooldown = 25f, Damage = 0, Range = 0f, StatusEffects = new() { "flying", "immune" }, Parameters = new() { ["duration"] = 1 } },
                new() { Id = "summon_drakes", Name = "Summon Drakes", Description = "Summons 2 mini-dragons", Type = AbilityType.Summon, Cooldown = 40f, Damage = 0, Range = 0f, StatusEffects = new(), Parameters = new() { ["summonCount"] = 2, ["summonType"] = "drake" } },
                new() { Id = "enrage", Name = "Enrage", Description = "+50% damage, +30% speed", Type = AbilityType.Buff, Cooldown = 999f, Damage = 0, Range = 0f, StatusEffects = new() { "enraged" }, Parameters = new() { ["damageBonus"] = 0.5f, ["speedBonus"] = 0.3f } }
            }
        };
    }

    private Dictionary<string, BossLoot> LoadLootDatabase()
    {
        return new Dictionary<string, BossLoot>
        {
            ["goblin_king"] = new()
            {
                BossId = "goblin_king",
                GuaranteedLoot = new()
                {
                    new() { ItemId = "crown_of_goblin_king", ItemName = "Crown of the Goblin King", Rarity = "Rare", DropChance = 1.0f, MinQuantity = 1, MaxQuantity = 1 }
                },
                RandomLoot = new()
                {
                    new() { ItemId = "goblin_blade", ItemName = "Goblin Blade", Rarity = "Uncommon", DropChance = 0.3f, MinQuantity = 1, MaxQuantity = 1 }
                },
                MinGold = 100,
                MaxGold = 200,
                ExperienceReward = 500
            },
            ["corrupted_treant"] = new()
            {
                BossId = "corrupted_treant",
                GuaranteedLoot = new()
                {
                    new() { ItemId = "bark_shield", ItemName = "Bark Shield", Rarity = "Epic", DropChance = 1.0f, MinQuantity = 1, MaxQuantity = 1 }
                },
                RandomLoot = new()
                {
                    new() { ItemId = "nature_essence", ItemName = "Nature Essence", Rarity = "Rare", DropChance = 0.5f, MinQuantity = 1, MaxQuantity = 3 }
                },
                MinGold = 150,
                MaxGold = 300,
                ExperienceReward = 800
            },
            ["flame_warden"] = new()
            {
                BossId = "flame_warden",
                GuaranteedLoot = new()
                {
                    new() { ItemId = "wardens_flame_staff", ItemName = "Warden's Flame Staff", Rarity = "Epic", DropChance = 1.0f, MinQuantity = 1, MaxQuantity = 1 }
                },
                RandomLoot = new()
                {
                    new() { ItemId = "flame_crystal", ItemName = "Flame Crystal", Rarity = "Rare", DropChance = 0.4f, MinQuantity = 1, MaxQuantity = 2 }
                },
                MinGold = 200,
                MaxGold = 400,
                ExperienceReward = 1200
            },
            ["shadow_assassin"] = new()
            {
                BossId = "shadow_assassin",
                GuaranteedLoot = new()
                {
                    new() { ItemId = "assassins_cloak", ItemName = "Assassin's Cloak", Rarity = "Legendary", DropChance = 1.0f, MinQuantity = 1, MaxQuantity = 1 }
                },
                RandomLoot = new()
                {
                    new() { ItemId = "shadow_essence", ItemName = "Shadow Essence", Rarity = "Epic", DropChance = 0.6f, MinQuantity = 1, MaxQuantity = 2 }
                },
                MinGold = 300,
                MaxGold = 500,
                ExperienceReward = 1500
            },
            ["ancient_dragon"] = new()
            {
                BossId = "ancient_dragon",
                GuaranteedLoot = new()
                {
                    new() { ItemId = "dragonscale_armor", ItemName = "Dragonscale Armor", Rarity = "Legendary", DropChance = 1.0f, MinQuantity = 1, MaxQuantity = 1 },
                    new() { ItemId = "dragon_heart", ItemName = "Dragon Heart", Rarity = "Legendary", DropChance = 1.0f, MinQuantity = 1, MaxQuantity = 1 }
                },
                RandomLoot = new()
                {
                    new() { ItemId = "dragon_scale", ItemName = "Dragon Scale", Rarity = "Epic", DropChance = 0.8f, MinQuantity = 3, MaxQuantity = 5 },
                    new() { ItemId = "dragon_tooth", ItemName = "Dragon Tooth", Rarity = "Rare", DropChance = 0.5f, MinQuantity = 1, MaxQuantity = 3 }
                },
                MinGold = 1000,
                MaxGold = 2000,
                ExperienceReward = 5000
            }
        };
    }

    private class BossDefinition
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public BossType Type { get; set; }
        public int BaseHP { get; set; }
        public int BaseDamage { get; set; }
        public List<string> AbilityIds { get; set; } = new();
        public List<BossPhase> Phases { get; set; } = new();
    }
}
