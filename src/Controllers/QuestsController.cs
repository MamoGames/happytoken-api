using HappyTokenApi.Data.Config;
using HappyTokenApi.Data.Core;
using HappyTokenApi.Data.Core.Entities;
using HappyTokenApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HappyTokenApi.Controllers
{
    [Route("[controller]")]
    public class QuestsController : DataController
    {
        //TODO: consider makes it better. This controller is used as a helper but is also a DataController.
        protected string m_UserId;

        public QuestsController(string userId, CoreDbContext coreDbContext, ConfigDbContext configDbContext) : base(coreDbContext, configDbContext)
        {
            this.m_UserId = userId;
        }

        public async Task<List<DbUserQuest>> CheckQuestUpdates()
        {
            var dbUserQuests = await m_CoreDbContext.UsersQuests
                .Where(i => i.UserId == this.m_UserId && i.IsActive)
                .ToListAsync();

            var updatedQuests = new List<DbUserQuest>();

            if (dbUserQuests?.Count > 0)
            {
                foreach (var dbUserQuest in dbUserQuests)
                {
                    if (dbUserQuest.ExpiryDate < DateTime.UtcNow)
                    {
                        dbUserQuest.Expire();

                        updatedQuests.Add(dbUserQuest);
                    }
                    else
                    {
                        if (!dbUserQuest.IsCompleted)
                        {
                            // check complete

                            bool allMet = true;

                            foreach (var requirement in dbUserQuest.TargetValues)
                            {
                                var dbUserStat = await m_CoreDbContext.UsersStats.Where(i => i.UserId == this.m_UserId && i.StatName == requirement.StatName).SingleOrDefaultAsync();

                                if (dbUserStat == null)
                                {
                                    allMet = false;
                                }
                                else
                                {
                                    if (dbUserStat.StatValue < requirement.StatValue)
                                    {
                                        allMet = false;
                                    }
                                }
                            }

                            if (allMet)
                            {
                                // complete quest
                                dbUserQuest.IsCompleted = true;

                                updatedQuests.Add(dbUserQuest);
                            }
                        }
                    }
                }
            }

            return updatedQuests;
        }

        // TODO: consider optimize the checking to only check those required
        public async Task<List<DbUserQuest>> CheckNewQuests()
        {
            var newQuests = new List<DbUserQuest>();

            var dbUserQuests = await m_CoreDbContext.UsersQuests
                .Where(i => i.UserId == this.m_UserId && i.IsActive)
                .ToListAsync();

            var allUserQuestIds = dbUserQuests.Select(i => i.QuestId).ToList();

            var allUserStats = await m_CoreDbContext.UsersStats
                .Where(i => i.UserId == this.m_UserId)
                .ToListAsync();

            // build list of quest with last finished time
            var dbFinishedUserQuests = await m_CoreDbContext.UsersQuests
                .Where(i => i.UserId == this.m_UserId && !i.IsActive && i.IsCompleted)
                .ToListAsync();
            var allFinishedQuestsWithTime = new Dictionary<string, DateTime>();

            foreach (var userQuest in dbFinishedUserQuests)
            {
                if (allFinishedQuestsWithTime.ContainsKey(userQuest.QuestId))
                {
                    if (userQuest.CreateDate > allFinishedQuestsWithTime[userQuest.QuestId]) allFinishedQuestsWithTime[userQuest.QuestId] = userQuest.CreateDate;
                }
                else
                {
                    allFinishedQuestsWithTime[userQuest.QuestId] = userQuest.CreateDate;
                }
            }

            foreach (var quest in m_ConfigDbContext.Quests.Quests)
            {
                // check quests that are not active right nove
                if (!allUserQuestIds.Contains(quest.QuestId))
                {
                    if (quest.ShouldTrigger(allUserStats.OfType<UserStat>().ToList(), allFinishedQuestsWithTime))
                    {
                        // new quest
                        var newQuest = new DbUserQuest
                        {
                            UsersQuestId = Guid.NewGuid().ToString(),
                            UserId = this.m_UserId,
                            QuestId = quest.QuestId,
                            CreateDate = DateTime.UtcNow,
                            IsActive = true,
                            Rewards = quest.QuestRewards.GenerateRewards(),
                            RequiresValues = quest.RequiresStat.ToArray(),
                            IsCompleted = false,
                            ExpiryDate = quest.TimeAllowed == 0 ? new DateTime(2100, 1, 1) : DateTime.UtcNow + new TimeSpan(0, 0, quest.TimeAllowed),
                        };

                        var targetValues = new List<UserStat>();

                        // calculate target value and expire time
                        foreach (var userStat in newQuest.RequiresValues)
                        {
                            var existUserStat = allUserStats.Find(i => i.StatName == userStat.StatName);

                            var targetUserStat = new UserStat
                            {
                                StatName = userStat.StatName,
                                StatValue = (existUserStat == null) ? userStat.StatValue : userStat.StatValue + existUserStat.StatValue,
                            };

                            targetValues.Add(targetUserStat);
                        }

                        newQuest.TargetValues = targetValues.ToArray();

                        await m_CoreDbContext.UsersQuests.AddAsync(newQuest);

                        newQuests.Add(newQuest);
                    }
                }
            }

            return newQuests;
        }
    }
}