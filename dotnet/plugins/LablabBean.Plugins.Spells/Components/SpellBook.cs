namespace LablabBean.Plugins.Spells.Components;

/// <summary>
/// Collection of known and equipped spells for an entity.
/// </summary>
public class SpellBook
{
    public HashSet<Guid> KnownSpells { get; set; } = new();
    public List<Guid> EquippedSpells { get; set; } = new();
    public int MaxEquippedSlots { get; set; } = 8;

    public bool KnowsSpell(Guid spellId) => KnownSpells.Contains(spellId);

    public bool LearnSpell(Guid spellId)
    {
        return KnownSpells.Add(spellId);
    }

    public bool EquipSpell(Guid spellId)
    {
        if (!KnowsSpell(spellId)) return false;
        if (EquippedSpells.Contains(spellId)) return false;
        if (EquippedSpells.Count >= MaxEquippedSlots) return false;

        EquippedSpells.Add(spellId);
        return true;
    }

    public bool UnequipSpell(Guid spellId)
    {
        return EquippedSpells.Remove(spellId);
    }

    public bool IsEquipped(Guid spellId) => EquippedSpells.Contains(spellId);
}
