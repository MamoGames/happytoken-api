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

        public List<DbUserQuest> CheckQuestUpdatesForUserStats(List<UserStat> updatedUserStats)
        {
            var updatedQuests = new List<DbUserQuest>();



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