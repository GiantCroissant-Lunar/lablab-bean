// Experience Component: Tracks player XP and leveling progress

namespace LablabBean.Plugins.Progression.Components
{
    /// <summary>
    /// Tracks experience points and leveling progress for an entity.
    /// </summary>
    public struct Experience
    {
        /// <summary>
        /// Current XP towards next level (resets on level-up).
        /// </summary>
        public int CurrentXP { get; set; }

        /// <summary>
        /// Current character level (starts at 1).
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Total XP required to reach next level.
        /// </summary>
        public int XPToNextLevel { get; set; }

        /// <summary>
        /// Total XP gained (lifetime statistic, never resets).
        /// </summary>
        public int TotalXPGained { get; set; }

        /// <summary>
        /// Creates a new Experience component at level 1.
        /// </summary>
        public static Experience CreateLevel1(int xpForLevel2)
        {
            return new Experience
            {
                CurrentXP = 0,
                Level = 1,
                XPToNextLevel = xpForLevel2,
                TotalXPGained = 0
            };
        }

        /// <summary>
        /// Calculates progress percentage to next level (0.0 to 1.0).
        /// </summary>
        public readonly float ProgressToNextLevel()
        {
            if (XPToNextLevel <= 0) return 0f;
            return (float)CurrentXP / XPToNextLevel;
        }

        /// <summary>
        /// Checks if ready to level up.
        /// </summary>
        public readonly bool IsLevelUpReady()
        {
            return CurrentXP >= XPToNextLevel;
        }

        /// <summary>
        /// Adds XP and returns overflow amount if level-up threshold reached.
        /// </summary>
        /// <returns>XP overflow if level-up needed, 0 otherwise</returns>
        public int AddXP(int amount)
        {
            CurrentXP += amount;
            TotalXPGained += amount;

            if (CurrentXP >= XPToNextLevel)
            {
                return CurrentXP - XPToNextLevel;
            }

            return 0;
        }

        /// <summary>
        /// Levels up the character and returns overflow XP.
        /// </summary>
        public int LevelUp(int nextLevelXPRequired)
        {
            int overflow = CurrentXP - XPToNextLevel;
            Level++;
            CurrentXP = 0;
            XPToNextLevel = nextLevelXPRequired;
            return overflow;
        }
    }
}
