using System.Collections.Generic;

namespace HappyTokenApi.Models
{
    public class Avatar
    {
        public AvatarType AvatarType { get; set; }

        public RarityType RarityType { get; set; }

        public HappinessType HappinessType { get; set; }

        public List<AvatarLevel> Levels { get; set; }
    }
}
