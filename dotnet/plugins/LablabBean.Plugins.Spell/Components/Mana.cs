namespace LablabBean.Plugins.Spell.Components;

/// <summary>
/// Component for tracking entity's mana resource.
/// </summary>
public struct Mana
{
    public int Current { get; set; }
    public int Max { get; set; }
    public int Regen { get; set; }

    public Mana(int max, int regen = 5)
    {
        Max = max;
        Current = max;
        Regen = regen;
    }

    public bool HasEnough(int amount) => Current >= amount;

    public void Consume(int amount)
    {
        Current = Math.Max(0, Current - amount);
    }

    public void Restore(int amount)
    {
        Current = Math.Min(Max, Current + amount);
    }

    public void RestoreFull()
    {
        Current = Max;
    }

    public float GetPercentage() => Max > 0 ? (float)Current / Max : 0f;
}
