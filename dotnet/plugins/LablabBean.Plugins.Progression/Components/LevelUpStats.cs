// LevelUpStats: Stat bonuses gained on level-up

namespace LablabBean.Plugins.Progression.Components
{
    /// <summary>
    /// Represents stat increases gained when leveling up.
    /// </summary>
    public struct LevelUpStats
    {
        /// <summary>
        /// Max health increase per level.
        /// </summary>
        public int HealthBonus { get; set; }

        /// <summary>
        /// Attack power increase per level.
        /// </summary>
        public int AttackBonus { get; set; }

        /// <summary>
        /// Defense increase per level.
        /// </summary>
        public int DefenseBonus { get; set; }

        /// <summary>
        /// Max mana increase per level.
        /// </summary>
        public int ManaBonus { get; set; }

        /// <summary>
        /// Speed/agility increase per level.
        /// </summary>
        public int SpeedBonus { get; set; }

        /// <summary>
        /// Creates default stat bonuses (balanced progression).
        /// </summary>
        public static LevelUpStats CreateDefault()
        {
            return new LevelUpStats
            {
                HealthBonus = 10,
                AttackBonus = 2,
                DefenseBonus = 1,
                ManaBonus = 5,
                SpeedBonus = 1
            };
        }

        /// <summary>
        /// Creates stat bonuses that scale with level.
        /// Higher levels get slightly better bonuses.
        /// </summary>
        public static LevelUpStats CreateScaled(int level)
        {
            // Every 10 levels, stats increase slightly
            int scaling = 1 + (level / 10);

            return new LevelUpStats
            {
                HealthBonus = 10 * scaling,
                AttackBonus = 2 * scaling,
                DefenseBonus = 1 * scaling,
                ManaBonus = 5 * scaling,
                SpeedBonus = 1 * scaling
            };
        }

        /// <summary>
        /// Checks if this contains any stat bonuses.
        /// </summary>
        public readonly bool HasAnyBonuses()
        {
            return HealthBonus > 0 || AttackBonus > 0 || DefenseBonus > 0 || ManaBonus > 0 || SpeedBonus > 0;
        }
    }
}
