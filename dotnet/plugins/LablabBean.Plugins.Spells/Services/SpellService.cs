using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.Spells.Components;
using LablabBean.Plugins.Spells.Data;
using LablabBean.Plugins.Spells.Systems;
using Microsoft.Extensions.Logging;
using SadRogue.Primitives;

namespace LablabBean.Plugins.Spells.Services;

/// <summary>
/// Service for managing spells, mana, and spell casting.
/// </summary>
public class SpellService
{
    private readonly World _world;
    private readonly ILogger _logger;
    private readonly Dictionary<Guid, Spell> _spellDatabase;
    private readonly ManaSystem _manaSystem;
    private readonly SpellCooldownSystem _cooldownSystem;
    private readonly SpellCastingSystem _castingSystem;

    public SpellService(
        World world,
        ILogger logger,
        Dictionary<Guid, Spell> spellDatabase,
        ManaSystem manaSystem,
        SpellCooldownSystem cooldownSystem,
        SpellCastingSystem castingSystem)
    {
        _world = world;
        _logger = logger;
        _spellDatabase = spellDatabase;
        _manaSystem = manaSystem;
        _cooldownSystem = cooldownSystem;
        _castingSystem = castingSystem;
    }

    #region Spell Casting

    public SpellCastResult CastSpell(
        Guid casterId,
        Guid spellId,
        Guid? targetId = null,
        int? targetX = null,
        int? targetY = null)
    {
        var caster = FindEntity(casterId);
        if (caster == null)
        {
            return new SpellCastResult(false, "Caster not found");
        }

        Entity? target = null;
        if (targetId.HasValue)
        {
            target = FindEntity(targetId.Value);
            if (target == null)
            {
                return new SpellCastResult(false, "Target not found");
            }
        }

        Point? targetPosition = null;
        if (targetX.HasValue && targetY.HasValue)
        {
            targetPosition = new Point(targetX.Value, targetY.Value);
        }

        return _castingSystem.CastSpell(caster.Value, spellId, target, targetPosition);
    }

    public bool CanCastSpell(Guid casterId, Guid spellId)
    {
        var caster = FindEntity(casterId);
        if (caster == null) return false;

        return _castingSystem.CanCastSpell(caster.Value, spellId);
    }

    public int GetSpellCooldown(Guid casterId, Guid spellId)
    {
        var caster = FindEntity(casterId);
        if (caster == null) return 0;

        return _cooldownSystem.GetRemainingCooldown(caster.Value, spellId);
    }

    #endregion

    #region Mana Management

    public ManaInfo GetMana(Guid entityId)
    {
        var entity = FindEntity(entityId);
        if (entity == null || !entity.Value.Has<Mana>())
        {
            return new ManaInfo(0, 0, 0, 0);
        }

        var mana = entity.Value.Get<Mana>();
        return new ManaInfo(mana.Current, mana.Maximum, mana.RegenRate, mana.CombatRegenRate);
    }

    public void RestoreMana(Guid entityId, int amount)
    {
        var entity = FindEntity(entityId);
        if (entity == null) return;

        _manaSystem.RestoreMana(entity.Value, amount);
    }

    public bool ConsumeMana(Guid entityId, int amount)
    {
        var entity = FindEntity(entityId);
        if (entity == null) return false;

        return _manaSystem.ConsumeMana(entity.Value, amount);
    }

    public void RegenerateMana(bool inCombat)
    {
        _manaSystem.RegenerateMana(inCombat);
    }

    #endregion

    #region Spell Learning

    public bool LearnSpell(Guid playerId, Guid spellId)
    {
        var player = FindEntity(playerId);
        if (player == null) return false;

        if (!player.Value.Has<SpellBook>())
        {
            player.Value.Add(new SpellBook());
        }

        ref var spellBook = ref player.Value.Get<SpellBook>();
        var learned = spellBook.LearnSpell(spellId);

        if (learned)
        {
            var spellName = _spellDatabase.TryGetValue(spellId, out var spell) ? spell.Name : "Unknown";
            _logger.LogInformation("Player {PlayerId} learned spell {SpellName}", playerId, spellName);
        }

        return learned;
    }

    public bool KnowsSpell(Guid playerId, Guid spellId)
    {
        var player = FindEntity(playerId);
        if (player == null || !player.Value.Has<SpellBook>())
            return false;

        var spellBook = player.Value.Get<SpellBook>();
        return spellBook.KnowsSpell(spellId);
    }

    public bool EquipSpell(Guid playerId, Guid spellId)
    {
        var player = FindEntity(playerId);
        if (player == null || !player.Value.Has<SpellBook>())
            return false;

        ref var spellBook = ref player.Value.Get<SpellBook>();
        var equipped = spellBook.EquipSpell(spellId);

        if (equipped)
        {
            var spellName = _spellDatabase.TryGetValue(spellId, out var spell) ? spell.Name : "Unknown";
            _logger.LogInformation("Player {PlayerId} equipped spell {SpellName}", playerId, spellName);
        }

        return equipped;
    }

    public void UnequipSpell(Guid playerId, Guid spellId)
    {
        var player = FindEntity(playerId);
        if (player == null || !player.Value.Has<SpellBook>())
            return;

        ref var spellBook = ref player.Value.Get<SpellBook>();
        spellBook.UnequipSpell(spellId);
    }

    #endregion

    #region Queries

    public IEnumerable<SpellInfo> GetKnownSpells(Guid playerId)
    {
        var player = FindEntity(playerId);
        if (player == null || !player.Value.Has<SpellBook>())
            return Enumerable.Empty<SpellInfo>();

        var spellBook = player.Value.Get<SpellBook>();
        return spellBook.KnownSpells
            .Where(id => _spellDatabase.ContainsKey(id))
            .Select(id => ToSpellInfo(_spellDatabase[id]));
    }

    public IEnumerable<SpellInfo> GetActiveSpells(Guid playerId)
    {
        var player = FindEntity(playerId);
        if (player == null || !player.Value.Has<SpellBook>())
            return Enumerable.Empty<SpellInfo>();

        var spellBook = player.Value.Get<SpellBook>();
        return spellBook.EquippedSpells
            .Where(id => _spellDatabase.ContainsKey(id))
            .Select(id => ToSpellInfo(_spellDatabase[id]));
    }

    public SpellInfo? GetSpellInfo(Guid spellId)
    {
        if (!_spellDatabase.TryGetValue(spellId, out var spell))
            return null;

        return ToSpellInfo(spell);
    }

    #endregion

    #region Helper Methods

    private Entity? FindEntity(Guid entityId)
    {
        // Simplified entity lookup - in production, use an entity registry service
        // For now, we'll iterate through all entities with Player or Enemy component
        var playerQuery = new QueryDescription().WithAll<Game.Core.Components.Player>();
        Entity? found = null;

        _world.Query(in playerQuery, (Entity entity) =>
        {
            // Match first player entity for player IDs
            if (found == null)
            {
                found = entity;
            }
        });

        if (found != null) return found;

        // If not found as player, search enemies
        var enemyQuery = new QueryDescription().WithAll<Game.Core.Components.Enemy>();
        _world.Query(in enemyQuery, (Entity entity) =>
        {
            // For MVP, return first enemy entity
            // TODO: Implement proper entity ID tracking
            if (found == null)
            {
                found = entity;
            }
        });

        return found;
    }

    private SpellInfo ToSpellInfo(Spell spell)
    {
        return new SpellInfo(
            spell.Id,
            spell.Name,
            spell.Description,
            spell.Type,
            spell.ManaCost,
            spell.Cooldown,
            spell.Targeting,
            spell.Range
        );
    }

    #endregion
}

/// <summary>
/// DTO for spell information.
/// </summary>
public record SpellInfo(
    Guid Id,
    string Name,
    string Description,
    SpellType Type,
    int ManaCost,
    int Cooldown,
    TargetingType Targeting,
    int Range
);

/// <summary>
/// DTO for mana status.
/// </summary>
public record ManaInfo(
    int Current,
    int Maximum,
    int RegenRate,
    int CombatRegenRate
);
