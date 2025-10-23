using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.Spell.Components;
using LablabBean.Plugins.Spell.Data;
using LablabBean.Plugins.Spell.Systems;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Spell.Services;

/// <summary>
/// Service for managing spells, spell learning, and spell casting.
/// </summary>
public class SpellService
{
    private readonly ILogger _logger;
    private readonly SpellCastingSystem _castingSystem;

    public SpellService(ILogger logger, SpellCastingSystem castingSystem)
    {
        _logger = logger;
        _castingSystem = castingSystem;
    }

    /// <summary>
    /// Register a spell in the spell library.
    /// </summary>
    public void RegisterSpell(Data.Spell spell)
    {
        _castingSystem.RegisterSpell(spell);
        _logger.LogInformation("Registered spell: {SpellName} (ID: {SpellId})", spell.Name, spell.SpellId);
    }

    /// <summary>
    /// Get a spell from the library by ID.
    /// </summary>
    public Data.Spell? GetSpell(string spellId)
    {
        return _castingSystem.GetSpell(spellId);
    }

    /// <summary>
    /// Teach a spell to an entity.
    /// </summary>
    public bool LearnSpell(Entity entity, string spellId)
    {
        var spell = _castingSystem.GetSpell(spellId);
        if (spell == null)
        {
            _logger.LogWarning("Cannot learn unknown spell: {SpellId}", spellId);
            return false;
        }

        var spellBook = entity.Has<SpellBook>() ? entity.Get<SpellBook>() : new SpellBook();

        if (spellBook.HasLearned(spellId))
        {
            _logger.LogDebug("Entity {Entity} already knows {SpellName}", entity, spell.Name);
            return false;
        }

        spellBook.LearnSpell(spellId);
        entity.Set(spellBook);

        _logger.LogInformation("Entity {Entity} learned {SpellName}", entity, spell.Name);
        return true;
    }

    /// <summary>
    /// Cast a spell from caster to target.
    /// </summary>
    public bool CastSpell(Entity caster, string spellId, Entity? target = null)
    {
        return _castingSystem.CastSpell(caster, spellId, target);
    }

    /// <summary>
    /// Get all spells learned by an entity.
    /// </summary>
    public List<Data.Spell> GetLearnedSpells(Entity entity)
    {
        if (!entity.Has<SpellBook>())
        {
            return new List<Data.Spell>();
        }

        var spellBook = entity.Get<SpellBook>();
        var spells = new List<Data.Spell>();

        foreach (var spellId in spellBook.LearnedSpells)
        {
            var spell = _castingSystem.GetSpell(spellId);
            if (spell != null)
            {
                spells.Add(spell);
            }
        }

        return spells;
    }

    /// <summary>
    /// Get all spells that are available to cast (not on cooldown, enough mana).
    /// </summary>
    public List<Data.Spell> GetAvailableSpells(Entity entity)
    {
        if (!entity.Has<SpellBook>() || !entity.Has<Mana>())
        {
            return new List<Data.Spell>();
        }

        var spellBook = entity.Get<SpellBook>();
        var mana = entity.Get<Mana>();
        var spells = new List<Data.Spell>();

        foreach (var spellId in spellBook.LearnedSpells)
        {
            var spell = _castingSystem.GetSpell(spellId);
            if (spell != null && CanCastSpell(entity, spell))
            {
                spells.Add(spell);
            }
        }

        return spells;
    }

    /// <summary>
    /// Check if an entity can cast a specific spell.
    /// </summary>
    public bool CanCastSpell(Entity entity, Data.Spell spell)
    {
        if (!entity.Has<SpellBook>() || !entity.Has<Mana>())
        {
            return false;
        }

        var spellBook = entity.Get<SpellBook>();
        var mana = entity.Get<Mana>();

        return spellBook.HasLearned(spell.SpellId) &&
               spellBook.IsSpellAvailable(spell.SpellId) &&
               mana.HasEnough(spell.ManaCost);
    }

    /// <summary>
    /// Get cooldown remaining for a spell.
    /// </summary>
    public int GetSpellCooldown(Entity entity, string spellId)
    {
        if (!entity.Has<SpellBook>())
        {
            return 0;
        }

        var spellBook = entity.Get<SpellBook>();
        return spellBook.GetCooldown(spellId);
    }

    /// <summary>
    /// Initialize mana component on an entity.
    /// </summary>
    public void InitializeMana(Entity entity, int maxMana, int regen = 5)
    {
        var mana = entity.Has<Mana>() ? entity.Get<Mana>() : new Mana(maxMana, regen);
        mana.Max = maxMana;
        mana.Regen = regen;
        mana.Current = maxMana;
        entity.Set(mana);

        _logger.LogDebug("Initialized mana for entity {Entity}: {Max} max, {Regen} regen", entity, maxMana, regen);
    }
}
