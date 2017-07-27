using System;

namespace HappyTokenApi.Models
{
    public class HappinessIndex
    {
        public int Wealth { get; set; }

        public int Social { get; set; }

        public int Health { get; set; }

        public int Experience { get; set; }

        public int Skill { get; set; }

        public int Total => Wealth + Social + Health + Experience + Skill;
    }
}
