using System.Collections.Generic;
using System.Linq;

namespace HappyTokenApi.Models
{
	public class Building
	{
		public BuildingType BuildingType { get; set; }

		public HappinessType HappinessType { get; set; }

		public RarityType RarityType { get; set; }

		public AvatarType AvatarType { get; set; }

		public AvatarType AvatarTypeAlt { get; set; }

		public List<BuildingLevel> Levels { get; set; }

        public int MaxLevel => Levels.Max(l => l.Level);

        public bool HasReachedMaxLevel(int currentLv = 1)
		{
			return MaxLevel == currentLv;		
		}
	}
}
