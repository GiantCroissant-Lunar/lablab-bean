using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Game.Core.Components;
using LablabBean.Plugins.Spells.Components;
using LablabBean.Plugins.Spells.Data;
using Microsoft.Extensions.Logging;
using SadRogue.Primitives;

namespace LablabBean.Plugins.Spells.Systems;

/// <summary>
/// System for spell casting validation and execution.
/// </summary>
public class SpellCastingSystem
{
    private readonly World _world;
    private readonly ILogger _logger;
    private readonly ManaSystem _manaSystem;
    private readonly SpellCooldownSystem _cooldownSystem;
    private readonly SpellEffectSystem _effectSystem;
    private readonly Dictionary<Guid, Spell> _spellDatabase;

    public SpellCastingSystem(
        World world,
        ILogger logger,
        ManaSystem manaSystem,
        SpellCooldownSystem cooldownSystem,
        SpellEffectSystem effectSystem,
        Dictionary<Guid, Spell> spellDatabase)
    {
        _world = world;
        _logger = logger;
        _manaSystem = manaSystem;
        _cooldownSystem = cooldownSystem;
        _effectSystem = effectSystem;
        _spellDatabase = spellDatabase;
    }

    public SpellCastResult CastSpell(
        Entity caster,
        Guid spellId,
        Entity? target = null,
        Point? targetPosition = null)
    {
        if (!_spellDatabase.TryGetValue(spellId, out var spell))
        {
            return new SpellCastResult(false, "Spell not found");
        }

        var validation = ValidateCast(caster, spell, target, targetPosition);
        if (!validation.Success)
        {
            return validation;
        }

        if (!_manaSystem.ConsumeMana(caster, spell.ManaCost))
        {
            return new SpellCastResult(false, "Insufficient mana");
        }

        if (spell.Cooldown > 0)
        {
            _cooldownSystem.StartCooldown(caster, spellId, spell.Cooldown);
        }

        var result = ExecuteSpell(caster, spell, target, targetPosition);

        _logger.LogInformation("Entity {Caster} cast spell {SpellName}",
            caster, spell.Name);

        return result;
    }

    private SpellCastResult ValidateCast(
        Entity caster,
        Spell spell,
        Entity? target,
        Point? targetPosition)
    {
        if (!caster.Has<SpellBook>())
        {
            return new SpellCastResult(false, "Caster has no spellbook");
        }

        var spellBook = caster.Get<SpellBook>();
        if (!spellBook.KnowsSpell(spell.Id))
        {
            return new SpellCastResult(false, "Spell not learned");
        }

        if (_cooldownSystem.IsOnCooldown(caster, spell.Id))
        {
            var remaining = _cooldownSystem.GetRemainingCooldown(caster, spell.Id);
            return new SpellCastResult(false, $"On cooldown ({remaining} turns)");
        }

        var (currentMana, _) = _manaSystem.GetMana(caster);
        if (currentMana < spell.ManaCost)
        {
            return new SpellCastResult(false, "Insufficient mana");
        }

        if (spell.Targeting == TargetingType.Single && target == null)
        {
            return new SpellCastResult(false, "No target specified");
        }

        if (spell.Targeting == TargetingType.AOE && targetPosition == null)
        {
            return new SpellCastResult(false, "No target position specified");
        }

        if (spell.Range > 0 && target != null)
        {
            if (!IsInRange(caster, target.Value, spell.Range))
            {
                return new SpellCastResult(false, "Target out of range");
            }
        }

        return new SpellCastResult(true);
    }

    private SpellCastResult ExecuteSpell(
        Entity caster,
        Spell spell,
        Entity? target,
        Point? targetPosition)
    {
        var affectedEntities = new List<Guid>();
        int totalDamage = 0;
        int totalHealing = 0;

        switch (spell.Targeting)
        {
            case TargetingType.Single:
                if (target.HasValue)
                {
                    var (damage, healing) = _effectSystem.ApplyEffects(
                        caster, target.Value, spell.Effects);
                    totalDamage += damage;
                    totalHealing += healing;
                    affectedEntities.Add(Guid.NewGuid()); // Track entity
                }
                break;

            case TargetingType.Self:
                var (selfDamage, selfHealing) = _effectSystem.ApplyEffects(
                    caster, caster, spell.Effects);
                totalDamage += selfDamage;
                totalHealing += selfHealing;
                affectedEntities.Add(Guid.NewGuid()); // Track entity
                break;

            case TargetingType.AOE:
                if (targetPosition.HasValue)
                {
                    var targets = GetEntitiesInRadius(targetPosition.Value, spell.AreaRadius);
                    foreach (var aoeTarget in targets)
                    {
                        var (damage, healing) = _effectSystem.ApplyEffects(
                            caster, aoeTarget, spell.Effects);
                        totalDamage += damage;
                        totalHealing += healing;
                        affectedEntities.Add(Guid.NewGuid()); // Track entity
                    }
                }
                break;

            case TargetingType.Directional:
                // TODO: Implement directional spell targeting
                _logger.LogWarning("Directional targeting not yet implemented");
                break;
        }

        return new SpellCastResult(
            true,
            DamageDealt: totalDamage,
            HealingDone: totalHealing,
            AffectedEntities: affectedEntities);
    }

    private bool IsInRange(Entity caster, Entity target, int range)
    {
        if (!caster.Has<Position>() || !target.Has<Position>())
            return false;

        var casterPos = caster.Get<Position>().Point;
        var targetPos = target.Get<Position>().Point;

        return Distance.Chebyshev.Calculate(casterPos, targetPos) <= range;
    }

    private List<Entity> GetEntitiesInRadius(Point center, int radius)
    {
        var entities = new List<Entity>();
        var positionQuery = new QueryDescription().WithAll<Position>();

        _world.Query(in positionQuery, (Entity entity, ref Position pos) =>
        {
            if (Distance.Chebyshev.Calculate(pos.Point, center) <= radius)
            {
                entities.Add(entity);
            }
        });

        return entities;
    }

    public bool CanCastSpell(Entity caster, Guid spellId)
    {
        if (!_spellDatabase.TryGetValue(spellId, out var spell))
            return false;

        if (!caster.Has<SpellBook>())
            return false;

        var spellBook = caster.Get<SpellBook>();
        if (!spellBook.KnowsSpell(spellId))
            return false;

        if (_cooldownSystem.IsOnCooldown(caster, spellId))
            return false;

        var (currentMana, _) = _manaSystem.GetMana(caster);
        return currentMana >= spell.ManaCost;
    }
}
