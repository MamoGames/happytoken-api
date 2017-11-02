using System;

namespace HappyTokenApi.Models
{
    public class Profile
    {
        public string Name { get; set; }

        public int Xp { get; set; }

        public int Level { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime LastSeenDate { get; set; }

        public DateTime LastDailyRewardDate { get; set; }

        public int GoldMineDaysRemaining { get; set; }

        public int GemMineDaysRemaining { get; set; }

        public int FriendCount { get; set; }

        public int CakeDonated { get; set; }

        /// <summary>
        /// Gain XP
        /// </summary>
        /// <returns><c>true</c>, if level up, <c>false</c> otherwise.</returns>
        /// <param name="xp">Xp.</param>
        public bool AddXp(int xp)
        {
            this.Xp += xp;

            // TODO: check level hp

            return false;
        }
    }
}
