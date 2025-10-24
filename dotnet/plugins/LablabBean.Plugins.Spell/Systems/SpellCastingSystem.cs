using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.Spell.Components;
using LablabBean.Plugins.Spell.Data;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Spell.Systems;

/// <summary>
/// System for casting spells and applying their effects.
/// </summary>
public class SpellCastingSystem
{
    private readonly World _world;
    private readonly ILogger _logger;
    private readonly Dictionary<string, Data.Spell> _spellLibrary;

    public SpellCastingSystem(World world, ILogger logger)
    {
        _world = world;
        _logger = logger;
        _spellLibrary = new Dictionary<string, Data.Spell>();
    }

    public void RegisterSpell(Data.Spell spell)
    {
        _spellLibrary[spell.SpellId] = spell;
    }

    public Data.Spell? GetSpell(string spellId)
    {
        return _spellLibrary.TryGetValue(spellId, out var spell) ? spell : null;
    }

    /// <summary>
    /// Attempt to cast a spell from caster to target.
    /// </summary>
    public bool CastSpell(Entity caster, string spellId, Entity? target = null)
    {
        if (!_spellLibrary.TryGetValue(spellId, out var spell))
        {
            _logger.LogWarning("Spell {SpellId} not found in library", spellId);
            return false;
        }

        if (!caster.Has<SpellBook>() || !caster.Has<Mana>())
        {
            _logger.LogWarning("Caster {Caster} missing SpellBook or Mana component", caster);
            return false;
        }

        var spellBook = caster.Get<SpellBook>();
        var mana = caster.Get<Mana>();

        if (!ValidateCast(spell, spellBook, mana))
        {
            return false;
        }

        mana.Consume(spell.ManaCost);
        caster.Set(mana);

        spellBook.StartCooldown(spellId, spell.Cooldown);
        caster.Set(spellBook);

        ApplySpellEffects(spell, caster, target);

        _logger.LogInformation("Spell {SpellName} cast by {Caster}", spell.Name, caster);
        return true;
    }

    private bool ValidateCast(Data.Spell spell, SpellBook spellBook, Mana mana)
    {
        if (!spellBook.HasLearned(spell.SpellId))
        {
            _logger.LogDebug("Spell {SpellId} not learned", spell.SpellId);
            return false;
        }

        if (!spellBook.IsSpellAvailable(spell.SpellId))
        {
            var cooldown = spellBook.GetCooldown(spell.SpellId);
            _logger.LogDebug("Spell {SpellId} on cooldown ({Turns} turns remaining)", spell.SpellId, cooldown);
            return false;
        }

        if (!mana.HasEnough(spell.ManaCost))
        {
            _logger.LogDebug("Not enough mana ({Current}/{Required})", mana.Current, spell.ManaCost);
            return false;
        }

        return true;
    }

    private void ApplySpellEffects(Data.Spell spell, Entity caster, Entity? target)
    {
        var actualTarget = spell.TargetType == TargetType.Self ? caster : target;

        if (actualTarget == null)
        {
            return;
        }

        switch (spell.SpellType)
        {
            case SpellType.Offensive:
                ApplyDamage(actualTarget.Value, spell.BaseDamage);
                break;
            case SpellType.Healing:
                ApplyHealing(actualTarget.Value, spell.BaseDamage);
                break;
            case SpellType.Buff:
            case SpellType.Debuff:
                ApplyBuffs(actualTarget.Value, spell);
                break;
        }

        foreach (var effectData in spell.Effects)
        {
            ApplyEffect(actualTarget.Value, effectData, spell.SpellId);
        }
    }

    private void ApplyDamage(Entity target, int damage)
    {
        // Placeholder - integrate with combat system
        _logger.LogInformation("Applied {Damage} damage to {Target}", damage, target);
    }

    private void ApplyHealing(Entity target, int healing)
    {
        // Placeholder - integrate with health system
        _logger.LogInformation("Healed {Target} for {Healing} HP", target, healing);
    }

    private void ApplyBuffs(Entity target, Data.Spell spell)
    {
        // Placeholder - integrate with stats system
        _logger.LogInformation("Applied buff {SpellName} to {Target}", spell.Name, target);
    }

    private void ApplyEffect(Entity target, SpellEffectData effectData, string sourceSpellId)
    {
        var effects = target.Has<ActiveEffects>() ? target.Get<ActiveEffects>() : new ActiveEffects();
        effects.AddEffect(new SpellEffect(
            effectData.EffectType,
            effectData.Duration,
            effectData.Intensity,
            sourceSpellId
        ));
        target.Set(effects);
    }
}
