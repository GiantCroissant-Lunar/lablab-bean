using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Game.Core.Components;
using Microsoft.Extensions.Logging;

namespace LablabBean.Game.Core.Systems;

/// <summary>
/// System that handles combat between entities
/// </summary>
public class CombatSystem
{
    private readonly ILogger<CombatSystem> _logger;
    private readonly Random _random;
    private readonly ItemSpawnSystem? _itemSpawnSystem;

    public CombatSystem(ILogger<CombatSystem> logger, ItemSpawnSystem? itemSpawnSystem = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = new Random();
        _itemSpawnSystem = itemSpawnSystem;
    }

    /// <summary>
    /// Executes an attack from attacker to defender
    /// Returns true if the attack was successful and dealt damage
    /// </summary>
    public bool Attack(World world, Entity attacker, Entity defender, StatusEffectSystem? statusEffectSystem = null)
    {
        if (!attacker.IsAlive() || !defender.IsAlive())
        {
            return false;
        }

        if (!attacker.Has<Combat>() || !defender.Has<Health>() || !defender.Has<Combat>())
        {
            _logger.LogWarning("Cannot execute attack - missing required components");
            return false;
        }

        ref var attackerCombat = ref attacker.Get<Combat>();
        ref var defenderCombat = ref defender.Get<Combat>();
        ref var defenderHealth = ref defender.Get<Health>();

        // Apply status effect modifiers to combat stats
        int modifiedAttack = GetModifiedAttack(attacker, attackerCombat.Attack);
        int modifiedDefense = GetModifiedDefense(defender, defenderCombat.Defense);

        // Calculate damage with modified stats
        int damage = CalculateDamage(modifiedAttack, modifiedDefense);

        if (damage <= 0)
        {
            _logger.LogDebug("Attack missed or was fully blocked");
            OnAttackMissed?.Invoke(attacker, defender);
            return false;
        }

        // Apply damage
        defenderHealth.Current = Math.Max(0, defenderHealth.Current - damage);

        string attackerName = GetEntityName(attacker);
        string defenderName = GetEntityName(defender);

        _logger.LogInformation("{Attacker} attacks {Defender} for {Damage} damage",
            attackerName, defenderName, damage);

        OnDamageDealt?.Invoke(attacker, defender, damage);

        // Try to apply status effect if attacker is an enemy with effect-inflicting attack
        if (attacker.Has<Enemy>() && statusEffectSystem != null)
        {
            var enemy = attacker.Get<Enemy>();
            if (enemy.InflictsEffect.HasValue && enemy.EffectProbability.HasValue)
            {
                // Roll for effect application
                int roll = _random.Next(100);
                if (roll < enemy.EffectProbability.Value)
                {
                    var result = statusEffectSystem.ApplyEffect(
                        world,
                        defender,
                        enemy.InflictsEffect.Value,
                        enemy.EffectMagnitude ?? EffectDefinitions.GetDefinition(enemy.InflictsEffect.Value).DefaultMagnitude,
                        enemy.EffectDuration ?? EffectDefinitions.GetDefinition(enemy.InflictsEffect.Value).DefaultDuration,
                        EffectSource.EnemyAttack
                    );

                    if (result.Success)
                    {
                        _logger.LogInformation("{AttackerName} inflicts {Effect} on {DefenderName}!",
                            attackerName, enemy.InflictsEffect.Value, defenderName);
                        OnStatusEffectApplied?.Invoke(attacker, defender, enemy.InflictsEffect.Value);
                    }
                }
            }
        }

        // Check if defender died
        if (defenderHealth.Current <= 0)
        {
            HandleDeath(world, defender, statusEffectSystem);
        }

        return true;
    }

    /// <summary>
    /// Calculates damage dealt based on attack and defense
    /// </summary>
    private int CalculateDamage(int attack, int defense)
    {
        // Random variance of ±20%
        float variance = 0.8f + (float)_random.NextDouble() * 0.4f;
        int baseDamage = (int)((attack - defense / 2) * variance);

        // Minimum damage is 0 (no negative damage)
        return Math.Max(0, baseDamage);
    }

    /// <summary>
    /// Handles entity death
    /// </summary>
    private void HandleDeath(World world, Entity entity, StatusEffectSystem? statusEffectSystem = null)
    {
        string entityName = GetEntityName(entity);
        _logger.LogInformation("{Entity} has been defeated!", entityName);

        OnEntityDied?.Invoke(entity);

        // Clear all status effects on death
        if (statusEffectSystem != null)
        {
            statusEffectSystem.ClearAllEffects(world, entity);
        }

        // Spawn loot for enemies
        if (entity.Has<Enemy>() && entity.Has<Position>() && _itemSpawnSystem != null)
        {
            var position = entity.Get<Position>();
            _itemSpawnSystem.SpawnEnemyLoot(world, position.Point, _random);
        }

        // Remove components that shouldn't exist on dead entities
        if (entity.Has<AI>())
        {
            entity.Remove<AI>();
        }

        if (entity.Has<Actor>())
        {
            entity.Remove<Actor>();
        }

        if (entity.Has<BlocksMovement>())
        {
            entity.Remove<BlocksMovement>();
        }

        // Change rendering to show a corpse
        if (entity.Has<Renderable>())
        {
            ref var renderable = ref entity.Get<Renderable>();
            renderable.Glyph = '%';
            renderable.Foreground = SadRogue.Primitives.Color.DarkRed;
        }
    }

    /// <summary>
    /// Heals an entity
    /// </summary>
    public void Heal(Entity entity, int amount)
    {
        if (!entity.IsAlive() || !entity.Has<Health>())
        {
            return;
        }

        ref var health = ref entity.Get<Health>();
        int oldHealth = health.Current;
        health.Current = Math.Min(health.Maximum, health.Current + amount);
        int actualHealing = health.Current - oldHealth;

        if (actualHealing > 0)
        {
            string entityName = GetEntityName(entity);
            _logger.LogInformation("{Entity} healed for {Amount} HP", entityName, actualHealing);
            OnHealed?.Invoke(entity, actualHealing);
        }
    }

    /// <summary>
    /// Gets the name of an entity for logging
    /// </summary>
    private string GetEntityName(Entity entity)
    {
        if (entity.Has<Name>())
        {
            return entity.Get<Name>().Value;
        }

        if (entity.Has<Player>())
        {
            return entity.Get<Player>().Name;
        }

        if (entity.Has<Enemy>())
        {
            return entity.Get<Enemy>().Type;
        }

        return $"Entity {entity.Id}";
    }

    /// <summary>
    /// Gets attack stat modified by status effects
    /// </summary>
    public int GetModifiedAttack(Entity entity, int baseAttack)
    {
        if (!entity.Has<StatusEffects>())
        {
            return baseAttack;
        }

        var statusEffects = entity.Get<StatusEffects>();
        int modifier = 0;

        foreach (var effect in statusEffects.ActiveEffects)
        {
            switch (effect.Type)
            {
                case EffectType.Strength:
                    modifier += effect.Magnitude;
                    break;
                case EffectType.Weakness:
                    modifier -= effect.Magnitude;
                    break;
            }
        }

        return Math.Max(1, baseAttack + modifier); // Minimum 1 attack
    }

    /// <summary>
    /// Gets defense stat modified by status effects
    /// </summary>
    public int GetModifiedDefense(Entity entity, int baseDefense)
    {
        if (!entity.Has<StatusEffects>())
        {
            return baseDefense;
        }

        var statusEffects = entity.Get<StatusEffects>();
        int modifier = 0;

        foreach (var effect in statusEffects.ActiveEffects)
        {
            switch (effect.Type)
            {
                case EffectType.IronSkin:
                    modifier += effect.Magnitude;
                    break;
                case EffectType.Fragile:
                    modifier -= effect.Magnitude;
                    break;
            }
        }

        return Math.Max(0, baseDefense + modifier); // Minimum 0 defense
    }

    /// <summary>
    /// Gets speed stat modified by status effects (for future use in turn order)
    /// </summary>
    public int GetModifiedSpeed(Entity entity, int baseSpeed)
    {
        if (!entity.Has<StatusEffects>())
        {
            return baseSpeed;
        }

        var statusEffects = entity.Get<StatusEffects>();
        int modifier = 0;

        foreach (var effect in statusEffects.ActiveEffects)
        {
            switch (effect.Type)
            {
                case EffectType.Haste:
                    modifier += effect.Magnitude;
                    break;
                case EffectType.Slow:
                    modifier -= effect.Magnitude;
                    break;
            }
        }

        return Math.Max(1, baseSpeed + modifier); // Minimum 1 speed
    }

    /// <summary>
    /// Event raised when damage is dealt
    /// </summary>
    public event Action<Entity, Entity, int>? OnDamageDealt;

    /// <summary>
    /// Event raised when an attack misses
    /// </summary>
    public event Action<Entity, Entity>? OnAttackMissed;

    /// <summary>
    /// Event raised when an entity dies
    /// </summary>
    public event Action<Entity>? OnEntityDied;

    /// <summary>
    /// Event raised when an entity is healed
    /// </summary>
    public event Action<Entity, int>? OnHealed;

    /// <summary>
    /// Event raised when a status effect is applied through combat
    /// </summary>
    public event Action<Entity, Entity, EffectType>? OnStatusEffectApplied;
}
