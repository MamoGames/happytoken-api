using System;
using System.Collections.Generic;
using System.Linq;

namespace HappyTokenApi.Models
{
    public class Avatar
    {
        public AvatarType AvatarType { get; set; }

        public RarityType RarityType { get; set; }

        public HappinessType HappinessType { get; set; }

        public List<AvatarLevel> Levels { get; set; }

        public int MaxLevel => Levels.Max(l => l.Level);

        public bool HasReachedMaxLevel(int currentLv = 1)
		{
			return MaxLevel == currentLv;
		}
    }
}
