using System;

namespace HappyTokenApi.Models
{
    public class Profile
    {
        public string Name { get; set; }

        public int Xp { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime LastSeenDate { get; set; }

        public int GoldMineDaysRemaining { get; set; }

        public int GemMineDaysRemaining { get; set; }
    }
}
