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
using System.Diagnostics;

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
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
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

        [Authorize]
        [HttpGet("friends")]
        public async Task<IActionResult> GetFriends()
        {
            var userId = this.GetClaimantUserId();

            // TODO: Change to join 
            // var results = (from p in persons
            //    join l in Location on p.PersonId equals l.PersonId
            //    where searchIds.Contains(l.Id)
            //    select p).Distinct().ToList();

            var dbUsersFriends = await m_CoreDbContext.UsersFriends
                .Where(i => i.UserId == userId)
                .ToListAsync();

            if (dbUsersFriends?.Count == 0)
            {
                return DataResult("friends", new List<FriendInfo>());
            }

            var dbUserProfiles = await (from usersProfile in m_CoreDbContext.UsersProfiles
                                        join usersFriend in m_CoreDbContext.UsersFriends on usersProfile.UserId equals usersFriend.FriendUserId
                                        where usersFriend.UserId == userId
                                        select usersProfile).ToListAsync();

            var dbUserHappiness = await (from usersHappiness in m_CoreDbContext.UsersHappiness
                                         join usersFriend in m_CoreDbContext.UsersFriends on usersHappiness.UserId equals usersFriend.FriendUserId
                                         where usersFriend.UserId == userId
                                         select usersHappiness).ToListAsync();


            var friends = new List<FriendInfo>();

            foreach (var dbUserFriend in dbUsersFriends)
            {
                var friend = new FriendInfo
                {
                    UserId = dbUserFriend.UserId,
                    FriendUserId = dbUserFriend.FriendUserId,
                    LastVisitDate = dbUserFriend.LastVisitDate,
                };

                var profile = dbUserProfiles.Find(i => i.UserId == dbUserFriend.FriendUserId);

                if (profile != null)
                {
                    friend.Level = profile.Level;
                    friend.Name = profile.Name;
                    friend.LastSeenDate = profile.LastSeenDate;
                    friend.CakeDonated = profile.CakeDonated;
                }

                var happiness = dbUserHappiness.Find(i => i.UserId == dbUserFriend.FriendUserId);

                if (happiness != null)
                {
                    friend.Happiness = happiness;
                }

                var dbUserAvatars = await m_CoreDbContext.UsersAvatars
                    .Where(i => i.UserId == dbUserFriend.FriendUserId)
                    .ToListAsync();

                var dbUserBuildings = await m_CoreDbContext.UsersBuildings
                    .Where(i => i.UserId == dbUserFriend.FriendUserId)
                    .ToListAsync();

                friend.UserAvatars = dbUserAvatars.OfType<UserAvatar>().ToList();
                friend.UserBuildings = dbUserBuildings.OfType<UserBuilding>().ToList();

                friends.Add(friend);
            }

            return DataResult("friends", friends);
        }

        [Authorize]
        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages()
        {
            var userId = this.GetClaimantUserId();

            if (!this.IsValidUserId(userId))
            {
                return BadRequest("UserId is invalid.");
            }

            var dbUsersMessages = await m_CoreDbContext.UsersMessages
                .Where(i => (i.ToUserId == null || i.ToUserId == userId) && !i.IsDeleted && i.ExpiryDate > DateTime.UtcNow)
                .ToListAsync();

            // get message status for adding status flags
            var dbUserMessageStatus = await m_CoreDbContext.UsersMessagesStatus.Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            if (dbUserMessageStatus != null)
            {
                // change status value that is not from the individual record table
                foreach (var message in dbUsersMessages)
                {
                    if (dbUserMessageStatus.ReadMessageIds.Contains(message.UsersMessageId))
                    {
                        message.IsRead = true;
                    }
                }

                // clean up ids that are no longer exist
                if (dbUserMessageStatus.CleanUp(dbUsersMessages.Select(i => i.UsersMessageId).ToList()))
                {
                    await m_CoreDbContext.SaveChangesAsync();
                }
            }

            var result = dbUsersMessages.OfType<UserMessage>().ToList();

            return DataResult("messages", result);
        }

        protected UserQuest GetUserQuest(DbUserQuest dbUserQuest)
        {
            var dbQuest = m_ConfigDbContext.Quests.Quests.Find(
                i => i.QuestId == dbUserQuest.QuestId);

            Debug.Assert(dbQuest != null, "Quest not in config");

            var userQuest = new UserQuest
            {
                Name = dbQuest.Name,
                Description = dbQuest.Description,
                QuestId = dbUserQuest.QuestId,
                IsCompleted = dbUserQuest.IsCompleted,
                CreateDate = dbUserQuest.CreateDate,
                ExpiryDate = dbUserQuest.ExpiryDate,
                RequiresValues = dbUserQuest.RequiresValues,
                TargetValues = dbUserQuest.TargetValues,
            };

            return userQuest;
        }
    }
}
