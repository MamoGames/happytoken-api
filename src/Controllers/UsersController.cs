using HappyTokenApi.Data.Config;
using HappyTokenApi.Data.Core;
using HappyTokenApi.Data.Core.Entities;
using HappyTokenApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTokenApi.Controllers
{
    [Route("[controller]")]
    public class UsersController : DataController
    {
        public UsersController(CoreDbContext coreDbContext, ConfigDbContext configDbContext) : base(coreDbContext, configDbContext)
        {
        }

        [Authorize]
        [HttpGet("login", Name = nameof(Login))]
        public async Task<IActionResult> Login()
        {
            var userId = this.GetClaimantUserId();

            if (!this.IsValidUserId(userId))
            {
                return BadRequest("UserId is invalid.");
            }

            //var questController = new QuestsController(this.GetClaimantUserId(), m_CoreDbContext, m_ConfigDbContext);
            //var updatedQuests = await questController.CheckQuestUpdates();
            //var newQuests = await questController.CheckNewQuests();

            //await m_CoreDbContext.SaveChangesAsync();

            this.AddDataToReturnList(await this.GetStatus());

            //if (updatedQuests.Count > 0 || newQuests.Count > 0)
            //{
            //    this.AddDataToReturnList(await this.GetUserQuests());
            //}

            // Send the updated buildings back to the user
            return RequestResult("");
        }


        [HttpPost]
        public async Task<IActionResult> CreateUserByDeviceId([FromBody] UserDevice userDevice)
        {
            if (string.IsNullOrEmpty(userDevice?.DeviceId))
            {
                return BadRequest("DeviceId was invalid.");
            }

            // NOTE: We may want to enable this again later
            // Check if DeviceId exists
            //var dbUser = await m_CoreDbContext.Users
            //    .Where(dbu => dbu.DeviceId == userDevice.DeviceId)
            //    .SingleOrDefaultAsync();

            // If it does not exist, create a new user
            //if (dbUser == null)
            //{
            var userId = Guid.NewGuid().ToString();
            var authToken = Guid.NewGuid().ToString();

            // User data
            var dbUser = new DbUser
            {
                UserId = userId,
                DeviceId = userDevice.DeviceId,
                AuthToken = authToken
            };

            // Users default profile
            var dbUserProfile = new DbUserProfile
            {
                UsersProfileId = Guid.NewGuid().ToString(),
                UserId = userId,
                Name = m_ConfigDbContext.UserDefaults.Profile.Name + new Random().Next(10000),
                Xp = m_ConfigDbContext.UserDefaults.Profile.Xp,
                CreateDate = DateTime.UtcNow,
                LastSeenDate = DateTime.UtcNow,
                LastDailyRewardDate = DateTime.UtcNow,
                GoldMineDaysRemaining = 0,
                GemMineDaysRemaining = 0,
                Level = 0,
                FriendCount = 0,
            };

            // Users default wallet
            var dbUserWallet = new DbUserWallet
            {
                UsersWalletId = Guid.NewGuid().ToString(),
                UserId = userId,
                HappyTokens = m_ConfigDbContext.UserDefaults.Wallet.HappyTokens,
                Gems = m_ConfigDbContext.UserDefaults.Wallet.Gems,
                Gold = m_ConfigDbContext.UserDefaults.Wallet.Gold,
            };

            // User default happiness
            var dbUserHappiness = new DbUserHappiness
            {
                UsersHappinessId = Guid.NewGuid().ToString(),
                UserId = userId,
                Wealth = m_ConfigDbContext.UserDefaults.Happiness.Wealth,
                Experience = m_ConfigDbContext.UserDefaults.Happiness.Experience,
                Health = m_ConfigDbContext.UserDefaults.Happiness.Health,
                Skill = m_ConfigDbContext.UserDefaults.Happiness.Skill,
                Social = m_ConfigDbContext.UserDefaults.Happiness.Social
            };

            // Create default avatars (Avatars give happiness, allocate based on Level1)
            var dbUsersAvatars = new List<DbUserAvatar>();
            foreach (var avatarType in m_ConfigDbContext.UserDefaults.AvatarTypes)
            {
                var userAvatar = new DbUserAvatar()
                {
                    UsersAvatarId = Guid.NewGuid().ToString(),
                    UserId = userId,
                    AvatarType = avatarType,
                    Level = 1,
                    Pieces = 0
                };

                // Grab the default avatar config
                var avatar = m_ConfigDbContext.Avatars.Avatars.Find(i => i.AvatarType == avatarType);

                // Add the happiness gained from Level1 to the users Happiness
                var happinessType = avatar.HappinessType;
                var happinessAmount = avatar.Levels[0].Happiness;
                dbUserHappiness.Add(happinessType, happinessAmount);

                dbUsersAvatars.Add(userAvatar);
            }

            // Create default buildings (Buildings give happiness, allocate based on Level1)
            var dbUsersBuildings = new List<DbUserBuilding>();
            int buildingCount = 0;
            foreach (var buildingType in m_ConfigDbContext.UserDefaults.BuildingTypes)
            {
                var userBuilding = new DbUserBuilding()
                {
                    UsersBuildingId = Guid.NewGuid().ToString(),
                    UserId = userId,
                    BuildingType = buildingType,
                    Level = 1,
                    Position = buildingCount,
                };

                buildingCount += 1;

                // Grab the default avatar config
                var building = m_ConfigDbContext.Buildings.Buildings.Find(i => i.BuildingType == buildingType);

                // Add the happiness gained from Level1 to the users Happiness
                var happinessType = building.HappinessType;
                var happinessAmount = building.Levels[0].Happiness;
                dbUserHappiness.Add(happinessType, happinessAmount);

                dbUsersBuildings.Add(userBuilding);
            }

            // Users default DailyActions
            var dbUserDailyActions = new DBUserDailyActions
            {
                UsersDailyActionId = Guid.NewGuid().ToString(),
                UserId = userId,
            };
            dbUserDailyActions.Update();

            // Add the new user
            await m_CoreDbContext.Users.AddAsync(dbUser);
            await m_CoreDbContext.UsersProfiles.AddAsync(dbUserProfile);
            await m_CoreDbContext.UsersWallets.AddAsync(dbUserWallet);
            await m_CoreDbContext.UsersHappiness.AddAsync(dbUserHappiness);
            await m_CoreDbContext.UsersAvatars.AddRangeAsync(dbUsersAvatars);
            await m_CoreDbContext.UsersBuildings.AddRangeAsync(dbUsersBuildings);
            await m_CoreDbContext.UsersDailyActions.AddAsync(dbUserDailyActions);

            // Save changes
            await m_CoreDbContext.SaveChangesAsync();

            // Create the user to send back to the client
            var response = new UserAuthPair
            {
                UserId = userId,
                AuthToken = authToken
            };

            return RequestResult(response);
            // }

            // User with this DeviceId already exists
            // return Forbid();
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
