using System;
using System.Linq;
using System.Threading.Tasks;
using HappyTokenApi.Data.Config;
using HappyTokenApi.Data.Core;
using HappyTokenApi.Data.Core.Entities;
using Microsoft.EntityFrameworkCore;
using HappyTokenApi.Models;
using System.Diagnostics;

namespace HappyTokenApi.Controllers
{
    public class UserStatsController : DataController
    {
        //TODO: consider makes it better. This controller is used as a helper but is also a DataController.
        protected string m_UserId;

        public UserStatsController(string userId, CoreDbContext coreDbContext, ConfigDbContext configDbContext) : base(coreDbContext, configDbContext)
        {
            this.m_UserId = userId;
        }

        public async Task AddUserStatValueAsync(UserStatType statType, long incValue)
        {
            //Debug.Assert(!statType.ToString().EndsWith("_", StringComparison.CurrentCultureIgnoreCase), "The user stat should not has an addition value");

            await this.AddUserStatValue(statType.ToString(), incValue);
        }

        public async Task AddUserStatValueAsync(UserStatType statType, string statNameValue, long incValue)
        {
            //Debug.Assert(statType.ToString().EndsWith("_", StringComparison.CurrentCultureIgnoreCase), "The user stat should has an addition value");

            await this.AddUserStatValue(string.Format("{0}{1}", statType.ToString(), statNameValue), incValue);
        }

        public async Task SetUserStatMaxValueAsync(UserStatType statType, long incValue)
        {
            //Debug.Assert(!statType.ToString().EndsWith("_", StringComparison.CurrentCultureIgnoreCase), "The user stat should not has an addition value");

            await this.SetUserStatMaxValue(statType.ToString(), incValue);
        }

        public async Task SetUserStatMaxValueAsync(UserStatType statType, string statNameValue, long incValue)
        {
            //Debug.Assert(statType.ToString().EndsWith("_", StringComparison.CurrentCultureIgnoreCase), "The user stat should not has an addition value");

            await this.SetUserStatMaxValue(string.Format("{0}{1}", statType.ToString(), statNameValue), incValue);
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
    }
}
