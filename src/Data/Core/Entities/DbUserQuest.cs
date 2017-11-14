using System;
using System.ComponentModel.DataAnnotations.Schema;
using HappyTokenApi.Models;
using Newtonsoft.Json;

namespace HappyTokenApi.Data.Core.Entities
{
    public class DbUserQuest
    {
        public string UserQuestId { get; set; }

        public string UserId { get; set; }

        public string QuestId { get; set; }

        public bool IsActive { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiryDate { get; set; }

        // json serialized value
        internal string _RequiresValues { get; set; }
        [NotMapped]
        public UserStat[] RequiresValues
        {
            get { return _RequiresValues == null ? null : JsonConvert.DeserializeObject<UserStat[]>(_RequiresValues); }
            set { _RequiresValues = JsonConvert.SerializeObject(value); }
        }

        // json serialized value
        internal string _TargetValues { get; set; }
        [NotMapped]
        public UserStat[] TargetValues
        {
            get { return _TargetValues == null ? null : JsonConvert.DeserializeObject<UserStat[]>(_TargetValues); }
            set { _TargetValues = JsonConvert.SerializeObject(value); }
        }
    }
}
