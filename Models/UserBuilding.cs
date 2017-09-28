using System;
using System.Collections.Generic;

namespace HappyTokenApi.Models
{
	public class UserBuilding
	{
		public BuildingType BuildingType { get; set; }

		public int Level { get; set; }

        public int Order { get; set; }
	}
}
