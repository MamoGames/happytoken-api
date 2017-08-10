using System;

namespace HappyTokenApi.Models
{
    public class Happiness
    {
        public int Experience { get; set; }

        public int Health { get; set; }

        public int Skill { get; set; }

        public int Social { get; set; }

        public int Wealth { get; set; }

        public int Total => Wealth + Social + Health + Experience + Skill;

        public void Add(HappinessType happinessType, int amount)
        {
            switch (happinessType)
            {
                case HappinessType.Experience: Experience += amount; return;
                case HappinessType.Health: Health += amount; return;
                case HappinessType.Skill: Skill += amount; return;
                case HappinessType.Social: Social += amount; return;
                case HappinessType.Wealth: Wealth += amount; return;
            }
        }
    }
}
