using System;
using System.Collections.Generic;
using HappyTokenApi.Models;

namespace HappyTokenApi.Data.Config.Entities
{
    public class DbQuestTriggerRequirements
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public List<UserStat> MinUserStats { get; set; }

        public QuestRepeatType RepeatType { get; set; }

        public List<string> PreQuestIds { get; set; }

        /// <summary>
        /// Indicates the hour for daily; week day for weekly; day of month for monthly
        /// </summary>
        /// <value>The period start value.</value>
        public int PeriodStartValue { get; set; }


        #region public functions

        public bool AllMet(string questId, List<UserStat> userStats, Dictionary<string, DateTime> allFinishedQuestsWithTime, string onCompleteQuestId = "")
        {
            var now = DateTime.UtcNow;

            if (now < StartDate || now > EndDate) return false;

            if (this.RepeatType == QuestRepeatType.OnceOnly && allFinishedQuestsWithTime.ContainsKey(questId))
            {
                return false;
            }

            // if pre quest is specified, only trigger when the quest is completed, also all other prequest should have been done
            if (this.PreQuestIds != null && this.PreQuestIds.Count > 0)
            {
                if (string.IsNullOrEmpty(onCompleteQuestId)) return false;

                // all the pre quest should have finished also
                foreach (var preQuestId in this.PreQuestIds)
                {
                    if (!allFinishedQuestsWithTime.ContainsKey(preQuestId)) return false;
                }
            }

            if (this.MinUserStats != null)
            {
                foreach (var minUserStat in this.MinUserStats)
                {
                    var userStat = userStats.Find(i => i.StatName == minUserStat.StatName);
                    if (userStat == null || userStat.StatValue < minUserStat.StatValue) return false;
                }
            }

            if (allFinishedQuestsWithTime.ContainsKey(questId) && this.RepeatType != QuestRepeatType.Repeating)
            {
                // check time for repeating
                switch (this.RepeatType)
                {
                    case QuestRepeatType.Daily:
                        var notBefore = new DateTime(now.Year, now.Month, now.Day, this.PeriodStartValue, 0, 0, 0);
                        if (now.Hour < this.PeriodStartValue)
                        {
                            notBefore = notBefore.AddDays(-1);
                        }

                        if (allFinishedQuestsWithTime[questId] >= notBefore) return false;

                        break;
                    case QuestRepeatType.Weekly:
                        notBefore = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, 0);
                        if ((int)now.DayOfWeek < this.PeriodStartValue)
                        {
                            // last week for start day
                            notBefore = notBefore.AddDays(-7 - (int)now.DayOfWeek + this.PeriodStartValue);
                        }
                        else if ((int)now.DayOfWeek > this.PeriodStartValue)
                        {
                            notBefore = notBefore.AddDays(this.PeriodStartValue - (int)now.DayOfWeek);
                        }

                        if (allFinishedQuestsWithTime[questId] >= notBefore) return false;

                        break;
                    case QuestRepeatType.Monthly:
                        notBefore = new DateTime(now.Year, now.Month, 1, 0, 0, 0, 0);
                        if (now.Day < this.PeriodStartValue)
                        {
                            // last month for start day
                            notBefore = new DateTime(now.Year, now.Month - 1, 1, 0, 0, 0, 0);
                        }

                        if (allFinishedQuestsWithTime[questId] >= notBefore) return false;

                        break;

                    default:
                        throw new Exception(string.Format("unhandled repeat type: {0}", this.RepeatType));

                }
            }

            return true;
        }


        #endregion public functions
    }
}

