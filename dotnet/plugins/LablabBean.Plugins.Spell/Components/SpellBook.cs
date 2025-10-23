using LablabBean.Plugins.Spell.Data;

namespace LablabBean.Plugins.Spell.Components;

/// <summary>
/// Component for tracking learned spells and cooldowns.
/// </summary>
public class SpellBook
{
    public List<string> LearnedSpells { get; set; } = new();
    public Dictionary<string, int> Cooldowns { get; set; } = new();

    public bool HasLearned(string spellId) => LearnedSpells.Contains(spellId);

    public void LearnSpell(string spellId)
    {
        if (!HasLearned(spellId))
        {
            LearnedSpells.Add(spellId);
        }
    }

    public bool IsSpellAvailable(string spellId)
    {
        if (!HasLearned(spellId)) return false;
        if (!Cooldowns.TryGetValue(spellId, out var cooldown)) return true;
        return cooldown <= 0;
    }

    public void StartCooldown(string spellId, int duration)
    {
        Cooldowns[spellId] = duration;
    }

    public int GetCooldown(string spellId)
    {
        return Cooldowns.TryGetValue(spellId, out var cooldown) ? cooldown : 0;
    }

    public void ReduceCooldowns(int amount = 1)
    {
        var keys = Cooldowns.Keys.ToList();
        foreach (var key in keys)
        {
            Cooldowns[key] = Math.Max(0, Cooldowns[key] - amount);
        }
    }
}
