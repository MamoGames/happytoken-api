using System;
using System.Linq;
using System.Threading.Tasks;
using HappyTokenApi.Data.Config;
using HappyTokenApi.Data.Core;
using HappyTokenApi.Data.Core.Entities;
using Microsoft.EntityFrameworkCore;
using HappyTokenApi.Models;
using System.Diagnostics;
using System.Collections.Generic;

namespace HappyTokenApi.Controllers
{
    public class UserStatsController : DataController
    {
        public List<string> UpdatedUserStatNames { get; private set; }

        //TODO: consider makes it better. This controller is used as a helper but is also a DataController.
        protected string m_UserId;

        public UserStatsController(string userId, CoreDbContext coreDbContext, ConfigDbContext configDbContext) : base(coreDbContext, configDbContext)
        {
            this.m_UserId = userId;

            this.UpdatedUserStatNames = new List<string>();
        }

        public async Task AddUserStatValueAsync(UserStatType statType, long incValue)
        {
            //Debug.Assert(!statType.ToString().EndsWith("_", StringComparison.CurrentCultureIgnoreCase), "The user stat should not has an addition value");

            var statName = statType.ToString();

            await this.AddUserStatValue(statName, incValue);

            this.AddUserStatToUpdatedList(statName);
        }

        public async Task AddUserStatValueAsync(UserStatType statType, string statNameValue, long incValue)
        {
            //Debug.Assert(statType.ToString().EndsWith("_", StringComparison.CurrentCultureIgnoreCase), "The user stat should has an addition value");

            var statName = string.Format("{0}{1}", statType.ToString(), statNameValue);

            await this.AddUserStatValue(statName, incValue);

            this.AddUserStatToUpdatedList(statName);
        }

        public async Task SetUserStatMaxValueAsync(UserStatType statType, long incValue)
        {
            //Debug.Assert(!statType.ToString().EndsWith("_", StringComparison.CurrentCultureIgnoreCase), "The user stat should not has an addition value");

            var statName = statType.ToString();

            await this.SetUserStatMaxValue(statName, incValue);

            this.AddUserStatToUpdatedList(statName);
        }

        public async Task SetUserStatMaxValueAsync(UserStatType statType, string statNameValue, long incValue)
        {
            //Debug.Assert(statType.ToString().EndsWith("_", StringComparison.CurrentCultureIgnoreCase), "The user stat should not has an addition value");

            var statName = string.Format("{0}{1}", statType.ToString(), statNameValue);

            await this.SetUserStatMaxValue(statName, incValue);

            this.AddUserStatToUpdatedList(statName);
        }

        protected async Task AddUserStatValue(string statName, long incValue)
        {
            var userStat = await this.GetUserStat(this.m_UserId, statName);
            userStat.AddValue(incValue);
        }

        protected async Task AddUserStatValue(string statName, float incValue)
        {
            var userStat = await this.GetUserStat(this.m_UserId, statName);
            userStat.AddValue(incValue);
        }

        protected async Task SetUserStatMaxValue(string statName, long incValue)
        {
            var userStat = await this.GetUserStat(this.m_UserId, statName);
            userStat.SetMaxValue(incValue);
        }

        protected async Task SetUserStatMaxValue(string statName, float incValue)
        {
            var userStat = await this.GetUserStat(this.m_UserId, statName);
            userStat.SetMaxValue(incValue);
        }

        private async Task<DbUserStat> GetUserStat(string userId, string statName)
        {
            var dbUserStat = await m_CoreDbContext.UsersStats.Where(i => i.UserId == userId && i.StatName == statName)
                .SingleOrDefaultAsync();

            if (dbUserStat == null)
            {
                // create the record for storing the data
                dbUserStat = new DbUserStat
                {
                    UsersStatId = Guid.NewGuid().ToString(),
                    UserId = userId,
                    StatName = statName,
                    StatValue = 0,
                };

                await m_CoreDbContext.UsersStats.AddAsync(dbUserStat);
            }

            return dbUserStat;
        }

        private void AddUserStatToUpdatedList(string statName)
        {
            if (!this.UpdatedUserStatNames.Contains(statName)) this.UpdatedUserStatNames.Add(statName);
        }
    }
}
