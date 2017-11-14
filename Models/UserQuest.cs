using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyTokenApi.Models
{
    public class UserQuest
    {
        public string QuestId { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        public UserStat[] RequiresValues { get; set; }

        // json serialized value
        public UserStat[] TargetValues { get; set; }
    }
}
