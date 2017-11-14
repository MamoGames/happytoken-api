using System;
using System.Collections.Generic;
using HappyTokenApi.Models;

namespace HappyTokenApi.Data.Config.Entities
{
    public class DBQuest
    {
        public string QuestId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public QuestTriggerRequirements TriggerRequirements { get; set; }

        public List<UserStat> RequiresStat { get; set; }

    }
}
