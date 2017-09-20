using System;

namespace HappyTokenApi.Models
{
    public class Friend
    {
        public string UserId { get; set; }

        public string FriendUserId { get; set; }

        public DateTime LastVisitDate { get; set; }
    }
}
