namespace LablabBean.Game.Core.Components;

/// <summary>
/// Component that represents an entity's gold currency
/// Used for merchant trading, quest rewards, and economy
/// </summary>
public struct Gold
{
    /// <summary>
    /// Current amount of gold held by the entity
    /// </summary>
    public int Amount { get; set; }

    public Gold(int amount = 0)
    {
        Amount = Math.Max(0, amount);
    }

    /// <summary>
    /// Adds gold to the current amount
    /// </summary>
    /// <param name="value">Amount to add</param>
    public void Add(int value)
    {
        if (value > 0)
        {
            Amount += value;
        }
    }

    /// <summary>
    /// Attempts to remove gold from the current amount
    /// </summary>
    /// <param name="value">Amount to remove</param>
    /// <returns>True if there was enough gold to remove, false otherwise</returns>
    public bool TryRemove(int value)
    {
        if (value <= 0 || Amount < value)
        {
            return false;
        }

        Amount -= value;
        return true;
    }

    /// <summary>
    /// Checks if the entity has at least the specified amount of gold
    /// </summary>
    /// <param name="value">Amount to check</param>
    /// <returns>True if entity has enough gold, false otherwise</returns>
    public bool Has(int value)
    {
        return Amount >= value;
    }

    public override string ToString()
    {
        return $"{Amount} gold";
    }
}
