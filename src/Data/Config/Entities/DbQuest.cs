using System;
using System.Collections.Generic;
using HappyTokenApi.Models;

namespace HappyTokenApi.Data.Config.Entities
{
    public class DbQuest
    {
        public string QuestId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Maximum number of seconds allowed to complete the game. Quest expire time mainly affect by this but also limited by other criterias
        /// </summary>
        /// <value>The time allowed.</value>
        public int TimeAllowed { get; set; }

        public DbQuestTriggerRequirements TriggerRequirements { get; set; }

        public List<UserStat> RequiresStat { get; set; }

        public QuestRewards QuestRewards { get; set; }


        #region public functions

        public bool ShouldTrigger(List<UserStat> userStats, Dictionary<string, DateTime> allFinishedQuestsWithTime, string onCompleteQuestId = "")
        {
            return this.TriggerRequirements.AllMet(this.QuestId, userStats, allFinishedQuestsWithTime, onCompleteQuestId);
        }

        #endregion public functions
    }
}
