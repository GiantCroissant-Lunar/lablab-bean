// Plugin Service Contract: Spell & Ability System
// Exposes spell casting, mana management, and spell learning via IPluginContext

using System;
using System.Collections.Generic;

namespace LablabBean.Plugins.Spells.Contracts
{
    /// <summary>
    /// Service for managing spells, mana, and spell casting.
    /// Registered in IPluginContext as "SpellService".
    /// </summary>
    public interface ISpellService
    {
        // Spell Casting

        /// <summary>
        /// Attempts to cast a spell.
        /// Validates mana cost, cooldown, targeting, and range.
        /// </summary>
        /// <param name="casterId">Entity casting spell</param>
        /// <param name="spellId">Spell to cast</param>
        /// <param name="targetId">Target entity (for Single targeting)</param>
        /// <param name="targetX">Target X coordinate (for AOE targeting)</param>
        /// <param name="targetY">Target Y coordinate (for AOE targeting)</param>
        /// <returns>Result of cast attempt</returns>
        SpellCastResult CastSpell(Guid casterId, Guid spellId, Guid? targetId = null, int? targetX = null, int? targetY = null);

        /// <summary>
        /// Checks if a spell can be cast (validates prerequisites).
        /// </summary>
        bool CanCastSpell(Guid casterId, Guid spellId);

        /// <summary>
        /// Gets remaining cooldown for a spell (in turns).
        /// </summary>
        int GetSpellCooldown(Guid casterId, Guid spellId);

        // Mana Management

        /// <summary>
        /// Gets current mana information.
        /// </summary>
        ManaInfo GetMana(Guid entityId);

        /// <summary>
        /// Restores mana (from potions, resting, etc.).
        /// </summary>
        void RestoreMana(Guid entityId, int amount);

        /// <summary>
        /// Consumes mana (manual deduction).
        /// </summary>
        /// <returns>True if entity had enough mana</returns>
        bool ConsumeMana(Guid entityId, int amount);

        /// <summary>
        /// Regenerates mana for all entities (called per turn).
        /// Uses CombatRegenRate if in combat, else RegenRate.
        /// </summary>
        void RegenerateMana(bool inCombat);

        // Spell Learning

        /// <summary>
        /// Teaches a spell to the player.
        /// </summary>
        bool LearnSpell(Guid playerId, Guid spellId);

        /// <summary>
        /// Checks if player knows a spell.
        /// </summary>
        bool KnowsSpell(Guid playerId, Guid spellId);

        /// <summary>
        /// Equips a spell to the hotbar.
        /// </summary>
        bool EquipSpell(Guid playerId, Guid spellId);

        /// <summary>
        /// Unequips a spell from the hotbar.
        /// </summary>
        void UnequipSpell(Guid playerId, Guid spellId);

        // Queries

        /// <summary>
        /// Gets all spells known by the player.
        /// </summary>
        IEnumerable<SpellInfo> GetKnownSpells(Guid playerId);

        /// <summary>
        /// Gets active (hotbar) spells.
        /// </summary>
        IEnumerable<SpellInfo> GetActiveSpells(Guid playerId);

        /// <summary>
        /// Gets spell details.
        /// </summary>
        SpellInfo GetSpellInfo(Guid spellId);
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

    /// <summary>
    /// Result of a spell cast attempt.
    /// </summary>
    public record SpellCastResult(
        bool Success,
        string? FailureReason  // "Insufficient mana", "On cooldown", "Out of range", etc.
    );

    public enum SpellType
    {
        Offensive,
        Defensive,
        Healing,
        Utility,
        AOE
    }

    public enum TargetingType
    {
        Single,
        AOE,
        Self,
        Directional
    }
}
