using System;
using HappyTokenApi.Data.Core.Entities;
using System.Linq;

namespace HappyTokenApi.Models
{
    public static class Extensions
    {
        #region UserStat

        public static void AddValue(this UserStat userStat, long value)
        {
            if (userStat.StatName.StartsWith("F_", StringComparison.CurrentCultureIgnoreCase))
            {
                userStat.StatValue += (value * 1000);
            }
            else
            {
                userStat.StatValue += value;
            }
        }

        public static void AddValue(this UserStat userStat, float value)
        {
            if (userStat.StatName.StartsWith("F_", StringComparison.CurrentCultureIgnoreCase))
            {
                userStat.StatValue += (long)(value * 1000 + 0.5f);
            }
            else
            {
                userStat.StatValue += (long)(value + 0.5f);
            }
        }

        public static void SetMaxValue(this UserStat userStat, long value)
        {
            if (userStat.StatName.StartsWith("F_", StringComparison.CurrentCultureIgnoreCase))
            {
                if ((value * 1000) > userStat.StatValue) userStat.StatValue = (value * 1000);
            }
            else
            {
                if (value > userStat.StatValue) userStat.StatValue = value;
            }
        }

        public static void SetMaxValue(this UserStat userStat, float value)
        {
            if (userStat.StatName.StartsWith("F_", StringComparison.CurrentCultureIgnoreCase))
            {
                if ((value * 1000 + 0.5f) > userStat.StatValue) userStat.StatValue = (long)(value * 1000 + 0.5f);
            }
            else
            {
                if (value > userStat.StatValue) userStat.StatValue = (long)(value + 0.5f);
            }
        }

        #endregion UserStat


        #region QuestRewards

        /// <summary>
        /// Generate a quest rewards without all the random values
        /// </summary>
        /// <returns>The quest rewards.</returns>
        public static Rewards GenerateRewards(this QuestRewards questRewards)
        {
            // TODO: support avatar pieces, etc

            return new Rewards
            {
                Wallet = new Wallet
                {
                    Gold = questRewards.Gold.GetNewRandomValue(),
                    Gems = questRewards.Gems.GetNewRandomValue(),
                    HappyTokens = questRewards.HappyTokens.GetNewRandomValue(),
                },
                Xp = questRewards.Xp.GetNewRandomValue(),
            };
        }

        #endregion QuestRewards


    }
}
