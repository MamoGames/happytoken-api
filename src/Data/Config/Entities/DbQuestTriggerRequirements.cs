using System;
using System.Collections.Generic;
using HappyTokenApi.Models;

namespace HappyTokenApi.Data.Config.Entities
{
    public class DbQuestTriggerRequirements
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public List<UserStat> MinUserStat { get; set; }

        public QuestRepeatType PeriodType { get; set; }

        public List<string> FinishedQuestIds { get; set; }

        public string OnFinishQuestId { get; set; }

        /// <summary>
        /// Indicates the hour for daily; week day for weekly, day of month for monthly
        /// </summary>
        /// <value>The period start day.</value>
        public int PeriodStartDay { get; set; }
    }
}

