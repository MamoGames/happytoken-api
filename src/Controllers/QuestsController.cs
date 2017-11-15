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
        public QuestsController(CoreDbContext coreDbContext, ConfigDbContext configDbContext) : base(coreDbContext, configDbContext)
        {
        }

        public async Task<List<DbUserQuest>> CheckQuestUpdatesForUserStats(List<UserStat> updatedUserStats)
        {
            var userId = this.GetClaimantUserId();

            var dbUserQuests = await m_CoreDbContext.UsersQuests
                .Where(i => i.UserId == userId && i.IsActive)
                .ToListAsync();

            var updatedQuests = new List<DbUserQuest>();

            if (dbUserQuests?.Count > 0)
            {
                foreach (var dbUserQuest in dbUserQuests)
                {
                    if (dbUserQuest.ExpiryDate < DateTime.UtcNow)
                    {
                        dbUserQuest.Expire();

                        updatedQuests.Append(dbUserQuest);
                    }
                    else
                    {
                        if (!dbUserQuest.IsCompleted)
                        {
                            // check complete

                            bool allMet = true;

                            foreach (var requirement in dbUserQuest.TargetValues)
                            {
                                var dbUserStat = await m_CoreDbContext.UsersStats.Where(i => i.UserId == userId && i.StatName == requirement.StatName).SingleOrDefaultAsync();

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

                                updatedQuests.Append(dbUserQuest);
                            }
                        }
                    }
                }
            }

            return updatedQuests;
        }

        // TODO: consider optimize the checking to only check those required
        public List<DbUserQuest> CheckNewQuests()
        {
            var newQuests = new List<DbUserQuest>();



            return newQuests;
        }
    }
}