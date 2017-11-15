using System;
namespace HappyTokenApi.Models
{
    public static class Extensions
    {
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
    }
}
