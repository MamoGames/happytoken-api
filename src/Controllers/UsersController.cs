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
    public class UsersController : Controller
    {
        private readonly CoreDbContext m_CoreDbContext;

        private readonly ConfigDbContext m_ConfigDbContext;

        public UsersController(CoreDbContext coreDbContext, ConfigDbContext configDbContext)
        {
            m_CoreDbContext = coreDbContext;
            m_ConfigDbContext = configDbContext;
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

            return Ok(response);
            // }

            // User with this DeviceId already exists
            // return Forbid();
        }

        [Authorize]
        [HttpGet("{userId}", Name = nameof(GetUserById))]
        public async Task<IActionResult> GetUserById(string userId)
        {
            if (!this.IsValidUserId(userId))
            {
                return BadRequest("UserId is invalid.");
            }

            if (!this.IsClaimantUserId(userId))
            {
                return Forbid();
            }

            var dbUser = await m_CoreDbContext.Users
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            if (dbUser != null)
            {
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

                // Clear out any expired messages
                var expiredMessages = await m_CoreDbContext.UsersMessages
                    .Where(i => i.ToUserId == userId && i.ExpiryDate > DateTime.UtcNow)
                    .ToListAsync();

                m_CoreDbContext.UsersMessages.RemoveRange(expiredMessages);

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

                return Ok(userLogin);
            }

            return NotFound("Could not find user.");
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
