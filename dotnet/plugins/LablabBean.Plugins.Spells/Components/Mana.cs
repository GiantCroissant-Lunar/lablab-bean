namespace LablabBean.Plugins.Spells.Components;

/// <summary>
/// Mana resource component for spell casting.
/// </summary>
public struct Mana
{
    public int Current { get; set; }
    public int Maximum { get; set; }
    public int RegenRate { get; set; }
    public int CombatRegenRate { get; set; }

    public Mana(int maximum, int regenRate = 5, int combatRegenRate = 2)
    {
        Current = maximum;
        Maximum = maximum;
        RegenRate = regenRate;
        CombatRegenRate = combatRegenRate;
    }

    public bool HasEnough(int amount) => Current >= amount;

    public bool Consume(int amount)
    {
        if (!HasEnough(amount)) return false;
        Current -= amount;
        return true;
    }

    public void Restore(int amount)
    {
        Current = Math.Min(Current + amount, Maximum);
    }

    public void RestoreFull()
    {
        Current = Maximum;
    }

    public float Percentage => Maximum > 0 ? (float)Current / Maximum : 0f;
}
