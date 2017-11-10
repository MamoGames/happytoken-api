using HappyTokenApi.Data.Config;
using HappyTokenApi.Data.Core;
using HappyTokenApi.Data.Core.Entities;
using HappyTokenApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HappyTokenApi.Controllers
{
    [Route("[controller]")]
    public class DataController : Controller
    {
        protected readonly CoreDbContext m_CoreDbContext;

        protected readonly ConfigDbContext m_ConfigDbContext;

        protected List<RecordData> updatedDataRecords;

        public DataController(CoreDbContext coreDbContext, ConfigDbContext configDbContext)
        {
            m_CoreDbContext = coreDbContext;
            m_ConfigDbContext = configDbContext;

            this.updatedDataRecords = new List<RecordData>();
        }

        protected void AddDataToReturnList(IActionResult obj)
        {
            var recordData = (obj as ObjectResult).Value as RecordData;

            // TODO: handle error    
            //if (recordData == null) 
            //{
            //    
            //}

            //string json = new JavaScriptSerializer().Serialize(jsonResult.Data);

            // TODO: remove existing record data with the same key

            this.updatedDataRecords.Add(recordData);
        }

        protected void ClearDataInReturnList()
        {
            this.updatedDataRecords.Clear();
        }

        protected IActionResult RequestResult(object content, int statusCode = 0)
        {
            return Ok(new RequestResult
            {
                Content = content,
                StatusCode = statusCode,
                Data = this.updatedDataRecords.ToArray(),
            });
        }

        protected IActionResult DataResult(string key, object record)
        {
            return Ok(new RecordData
            {
                Key = key,
                Hash = "",
                Data = record,
            });
        }

        [Authorize]
        [HttpGet("cake")]
        public async Task<IActionResult> Cake()
        {
            return DataResult("cake", new Cake
            {
                Gold = 10,
            });
        }

        [Authorize]
        [HttpGet("status")]
        public async Task<IActionResult> Status()
        {
            var userId = this.GetClaimantUserId();

            if (!this.IsValidUserId(userId))
            {
                return BadRequest("UserId is invalid.");
            }

            var dbUser = await m_CoreDbContext.Users
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            if (dbUser == null) return BadRequest("User record not found");

            var dbUserProfile = await m_CoreDbContext.UsersProfiles
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            var dbUserWallet = await m_CoreDbContext.UsersWallets
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            var dbUserHappiness = await m_CoreDbContext.UsersHappiness
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            var dbUserAvatars = await m_CoreDbContext.UsersAvatars
                .Where(i => i.UserId == userId)
                .ToListAsync();

            var dbUserBuildings = await m_CoreDbContext.UsersBuildings
                .Where(i => i.UserId == userId)
                .ToListAsync();

            var dbUserCakes = await m_CoreDbContext.UsersCakes
                .Where(i => i.UserId == userId)
                .ToListAsync();

            var dbUserStorePurchaseRecords = await m_CoreDbContext.UsersStorePurchaseRecords
                .Where(i => i.UserId == userId)
                .ToListAsync();

            var dbUserDailyActions = await m_CoreDbContext.UsersDailyActions
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            // Update the Daily actions, if we have any
            dbUserDailyActions?.Update();

            // Check if we give the players their daily reward
            var dailyRewards = ProcessDailyReward(dbUserProfile, dbUserWallet);

            var userLogin = new UserLogin
            {
                UserId = userId,
                Profile = dbUserProfile,
                Wallet = dbUserWallet,
                Happiness = dbUserHappiness,
                UserAvatars = dbUserAvatars.OfType<UserAvatar>().ToList(),
                UserBuildings = dbUserBuildings.OfType<UserBuilding>().ToList(),
                UserCakes = dbUserCakes.OfType<UserCake>().ToList(),
                UserStorePurchaseRecords = dbUserStorePurchaseRecords.OfType<UserStorePurchaseRecord>().ToList(),
                UserDailyActions = dbUserDailyActions,
                DailyRewards = dailyRewards
            };

            dbUserProfile.LastSeenDate = DateTime.UtcNow;

            await m_CoreDbContext.SaveChangesAsync();

            return DataResult("status", userLogin);
        }

        private DailyRewards ProcessDailyReward(Profile profile, Wallet wallet)
        {
            var dailyRewards = new DailyRewards();

            var hoursSinceLastReward = DateTime.UtcNow - profile.LastDailyRewardDate;

            if (hoursSinceLastReward.TotalHours >= 24)
            {
                profile.LastDailyRewardDate = DateTime.UtcNow;

                if (profile.GoldMineDaysRemaining > 0)
                {
                    var goldMine = m_ConfigDbContext.Store.ResourceMines.Find(i => i.ResourceMineType == ResourceMineType.Gold);

                    profile.GoldMineDaysRemaining--;
                    wallet.Gold += goldMine.AmountPerDay;
                    dailyRewards.Wallet.Gold = goldMine.AmountPerDay;
                }

                if (profile.GemMineDaysRemaining > 0)
                {
                    var gemMine = m_ConfigDbContext.Store.ResourceMines.Find(i => i.ResourceMineType == ResourceMineType.Gems);

                    profile.GemMineDaysRemaining--;
                    wallet.Gems += gemMine.AmountPerDay;
                    dailyRewards.Wallet.Gems = gemMine.AmountPerDay;
                }
            }

            return dailyRewards;
        }
    }
}
