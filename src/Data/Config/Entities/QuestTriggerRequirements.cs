using System;
using System.Collections.Generic;
using HappyTokenApi.Models;

namespace HappyTokenApi.Data.Config.Entities
{
    public class QuestTriggerRequirements
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int MinUserLevel { get; set; }

        public List<UserStat> MinUserStat { get; set; }
    }
}

