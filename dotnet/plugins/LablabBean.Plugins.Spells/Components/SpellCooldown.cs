namespace LablabBean.Plugins.Spells.Components;

/// <summary>
/// Tracks active cooldowns for cast spells.
/// </summary>
public class SpellCooldown
{
    public Dictionary<Guid, int> ActiveCooldowns { get; set; } = new();

    public bool IsOnCooldown(Guid spellId)
    {
        return ActiveCooldowns.TryGetValue(spellId, out var remaining) && remaining > 0;
    }

    public int GetRemainingCooldown(Guid spellId)
    {
        return ActiveCooldowns.TryGetValue(spellId, out var remaining) ? remaining : 0;
    }

    public void StartCooldown(Guid spellId, int turns)
    {
        ActiveCooldowns[spellId] = turns;
    }

    public void DecrementAll()
    {
        var keys = ActiveCooldowns.Keys.ToList();
        foreach (var key in keys)
        {
            ActiveCooldowns[key]--;
            if (ActiveCooldowns[key] <= 0)
            {
                ActiveCooldowns.Remove(key);
            }
        }
    }
}
