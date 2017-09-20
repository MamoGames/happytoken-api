using System;

namespace HappyTokenApi.Models
{
    public class FriendInfo : Friend
    {
        public DateTime LastSeenDate { get; set; }

        public string Name { get; set; }

        public Happiness Happiness { get; set; }

        public int Level { get; set; }
    }
}
