using System;
using System.Collections.Generic;

namespace HappyTokenApi.Models
{
    public class FriendInfo : Friend
    {
        public DateTime LastSeenDate { get; set; }

        public string Name { get; set; }

        public Happiness Happiness { get; set; }

        public int Level { get; set; }

		public List<UserAvatar> UserAvatars { get; set; }

		public List<UserBuilding> UserBuildings { get; set; }
    }
}
