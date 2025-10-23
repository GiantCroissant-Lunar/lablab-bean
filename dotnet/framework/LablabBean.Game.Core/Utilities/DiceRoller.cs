namespace LablabBean.Game.Core.Utilities;

/// <summary>
/// Utility class for performing skill checks and dice rolls
/// </summary>
public static class DiceRoller
{
    private static readonly Random _random = new();

    /// <summary>
    /// Rolls a single die with the specified number of sides
    /// </summary>
    /// <param name="sides">Number of sides on the die (e.g., 6 for d6, 20 for d20)</param>
    /// <returns>Result of the roll (1 to sides)</returns>
    public static int Roll(int sides)
    {
        if (sides <= 0)
            throw new ArgumentException("Die must have at least 1 side", nameof(sides));

        return _random.Next(1, sides + 1);
    }

    /// <summary>
    /// Rolls multiple dice and returns the sum
    /// </summary>
    /// <param name="count">Number of dice to roll</param>
    /// <param name="sides">Number of sides on each die</param>
    /// <returns>Sum of all dice rolls</returns>
    public static int Roll(int count, int sides)
    {
        if (count <= 0)
            throw new ArgumentException("Must roll at least 1 die", nameof(count));

        int total = 0;
        for (int i = 0; i < count; i++)
        {
            total += Roll(sides);
        }
        return total;
    }

    /// <summary>
    /// Rolls dice with a modifier (e.g., "2d6+3")
    /// </summary>
    /// <param name="count">Number of dice to roll</param>
    /// <param name="sides">Number of sides on each die</param>
    /// <param name="modifier">Modifier to add to the result</param>
    /// <returns>Sum of dice rolls plus modifier</returns>
    public static int Roll(int count, int sides, int modifier)
    {
        return Roll(count, sides) + modifier;
    }

    /// <summary>
    /// Performs a skill check with advantage/disadvantage
    /// </summary>
    /// <param name="skillModifier">Character's skill modifier</param>
    /// <param name="difficulty">DC (Difficulty Class) to beat</param>
    /// <param name="advantage">True for advantage (roll twice, take higher)</param>
    /// <param name="disadvantage">True for disadvantage (roll twice, take lower)</param>
    /// <returns>True if check succeeds, false otherwise</returns>
    public static bool SkillCheck(int skillModifier, int difficulty, bool advantage = false, bool disadvantage = false)
    {
        int roll;

        if (advantage && !disadvantage)
        {
            int roll1 = Roll(20);
            int roll2 = Roll(20);
            roll = Math.Max(roll1, roll2);
        }
        else if (disadvantage && !advantage)
        {
            int roll1 = Roll(20);
            int roll2 = Roll(20);
            roll = Math.Min(roll1, roll2);
        }
        else
        {
            roll = Roll(20);
        }

        int total = roll + skillModifier;
        return total >= difficulty;
    }

    /// <summary>
    /// Rolls for a percentage check (0-100)
    /// </summary>
    /// <returns>Result from 1 to 100</returns>
    public static int RollPercentage()
    {
        return _random.Next(1, 101);
    }

    /// <summary>
    /// Checks if a percentage-based event occurs
    /// </summary>
    /// <param name="chance">Chance of success (0-100)</param>
    /// <returns>True if event occurs, false otherwise</returns>
    public static bool PercentageCheck(int chance)
    {
        if (chance <= 0) return false;
        if (chance >= 100) return true;

        return RollPercentage() <= chance;
    }
}
