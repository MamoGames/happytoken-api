﻿using HappyTokenApi.Data.Config;
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
    public class FriendsController : DataController
    {
        public FriendsController(CoreDbContext coreDbContext, ConfigDbContext configDbContext) : base(coreDbContext, configDbContext)
        {
        }

        [Authorize]
        [HttpGet("suggested", Name = nameof(GetSuggestedFriends))]
        public async Task<IActionResult> GetSuggestedFriends()
        {
            var userId = this.GetClaimantUserId();

            var fromDate = DateTime.UtcNow - TimeSpan.FromHours(24);

            var dbUserProfiles = await m_CoreDbContext.UsersProfiles
                .Where(i => i.LastSeenDate > fromDate)
                .Take(20)
                .ToListAsync();

            var friends = new List<FriendInfo>();

            foreach (var profile in dbUserProfiles)
            {
                // exclude self
                if (profile.UserId == userId) continue;

                var friendInfo = new FriendInfo
                {
                    UserId = userId,
                    FriendUserId = profile.UserId,
                    Name = profile.Name,
                    LastSeenDate = profile.LastSeenDate,
                    LastVisitDate = DateTime.MinValue,
                    Level = profile.Level,
                    CakeDonated = profile.CakeDonated,
                    Happiness = new Happiness() // Do we want Happiness?
                };

                friends.Add(friendInfo);
            }

            return RequestResult(friends);
        }

        [Authorize]
        [HttpPost("", Name = nameof(AddFriendByUserId))]
        public async Task<IActionResult> AddFriendByUserId([FromBody] string friendUserId)
        {
            if (string.IsNullOrEmpty(friendUserId))
            {
                return BadRequest("Friend UserId was invalid.");
            }

            // TODO: This seems to be choking on the friends UserId, which is a valid GUID
            //if (this.IsValidUserId(friendUserId))
            //{
            //    return Forbid("Friend UserId is not valid.");
            //}

            var userId = this.GetClaimantUserId();

            // Check users friend count
            var dbUserProfile = await m_CoreDbContext.UsersProfiles
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            if (dbUserProfile == null)
            {
                return BadRequest("Profile for UserId was not found.");
            }

            if (dbUserProfile.FriendCount >= 20)
            {
                return BadRequest("User has too many friends.");
            }

            if (userId == friendUserId)
            {
                return BadRequest("Cannot add self as friend.");
            }

            var dbUserFriends = await m_CoreDbContext.UsersFriends
                .Where(i => i.UserId == userId)
                .ToListAsync();

            if (dbUserFriends != null)
            {
                // Check user is not already friend in either direction
                if (dbUserFriends.Any(i => i.FriendUserId == friendUserId))
                {
                    return BadRequest("User is already friends.");
                }
            }

            // Add user and friends relationship
            var userToFriend = new DbUserFriend
            {
                UsersFriendId = Guid.NewGuid().ToString(),
                UserId = userId,
                FriendUserId = friendUserId,
                LastVisitDate = DateTime.UtcNow
            };

            await m_CoreDbContext.UsersFriends.AddAsync(userToFriend);

            // Add friend and users relationship
            var friendToUser = new DbUserFriend
            {
                UsersFriendId = Guid.NewGuid().ToString(),
                UserId = userToFriend.FriendUserId,
                FriendUserId = userToFriend.UserId,
                LastVisitDate = DateTime.UtcNow
            };

            await m_CoreDbContext.UsersFriends.AddAsync(friendToUser);

            // Increment the Users friend count
            dbUserProfile.FriendCount++;

            //TODO: add friend count for target

            await m_CoreDbContext.SaveChangesAsync();

            this.AddDataToReturnList(await this.GetUserFriends());

            // Grab the entire (updated) list of friends, and return to the user
            return RequestResult("");
        }

        [Authorize]
        [HttpPost("remove", Name = nameof(RemoveFriendByUserId))]
        public async Task<IActionResult> RemoveFriendByUserId([FromBody] string friendUserId)
        {
            if (string.IsNullOrEmpty(friendUserId))
            {
                return BadRequest("Friend UserId was invalid.");
            }

            // TODO: This seems to be choking on the friends UserId, which is a valid GUID
            //if (this.IsValidUserId(friendUserId))
            //{
            //    return Forbid("Friend UserId is not valid.");
            //}

            var userId = this.GetClaimantUserId();

            // Check users friend count
            var dbUserProfile = await m_CoreDbContext.UsersProfiles
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            if (dbUserProfile == null)
            {
                return BadRequest("Profile for UserId was not found.");
            }

            var dbUserFriends = await m_CoreDbContext.UsersFriends
                .Where(i => i.UserId == userId)
                .ToListAsync();

            if (dbUserFriends == null) return BadRequest("User is not in friend list.");

            // Check user is not already friend in either direction
            var userFriend = dbUserFriends.Find(i => i.FriendUserId == friendUserId);
            if (userFriend == null)
            {
                return BadRequest("User is not in friend list.");
            }

            // Remove user and friends relationship
            m_CoreDbContext.UsersFriends.Remove(userFriend);

            // Remove friend and user's relationship
            userFriend = await m_CoreDbContext.UsersFriends
                .Where(i => i.UserId == friendUserId && i.FriendUserId == userId)
                .SingleAsync();

            m_CoreDbContext.UsersFriends.Remove(userFriend);

            // Increment the Users friend count
            dbUserProfile.FriendCount--;

            //TODO: remove friend count from target

            await m_CoreDbContext.SaveChangesAsync();

            // Grab the entire (updated) list of friends, and return to the user
            this.AddDataToReturnList(await this.GetUserFriends());
            return RequestResult("");
        }

        [Authorize]
        [HttpPost("search", Name = nameof(SearchUsers))]
        public async Task<IActionResult> SearchUsers([FromBody] string userName)
        {
            var userId = this.GetClaimantUserId();

            var fromDate = DateTime.UtcNow - TimeSpan.FromHours(24);

            var dbUserProfiles = await m_CoreDbContext.UsersProfiles
                .Where(i => i.Name.ToLower().Contains(userName.ToLower()))
                .Take(20)
                .ToListAsync();

            var friends = new List<FriendInfo>();

            foreach (var profile in dbUserProfiles)
            {
                // exclude self
                if (profile.UserId == userId) continue;

                var friendInfo = new FriendInfo
                {
                    UserId = userId,
                    FriendUserId = profile.UserId,
                    Name = profile.Name,
                    LastSeenDate = profile.LastSeenDate,
                    LastVisitDate = DateTime.MinValue,
                    Level = profile.Level,
                    CakeDonated = profile.CakeDonated,
                    Happiness = new Happiness() // Do we want Happiness?
                };

                friends.Add(friendInfo);
            }

            return RequestResult(friends);
        }

        [Authorize]
        [HttpPost("giftcake", Name = nameof(GiftCakeToFriend))]
        public async Task<IActionResult> GiftCakeToFriend([FromBody] UserSendCakeMessage sendCakeMessage)
        {
            var friendUserId = sendCakeMessage.ToUserId;
            if (string.IsNullOrEmpty(friendUserId))
            {
                return BadRequest("Friend UserId was invalid.");
            }

            var userId = this.GetClaimantUserId();

            // Check users friend count
            var dbUserProfile = await m_CoreDbContext.UsersProfiles
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            if (dbUserProfile == null)
            {
                return BadRequest("Profile for UserId was not found.");
            }

            var dbUserFriend = await m_CoreDbContext.UsersFriends
                .Where(i => i.UserId == userId && i.FriendUserId == friendUserId)
                .SingleOrDefaultAsync();

            if (dbUserFriend == null) return BadRequest("User is not in friend list.");

            // check if the user has already gifted the friend or has reached the max number of gift today
            var dbUserDailyActions = await m_CoreDbContext.UsersDailyActions
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            if (dbUserDailyActions == null) return BadRequest("User daily actions record not found.");
            dbUserDailyActions.Update();

            if (dbUserDailyActions.GiftedCakeUserIds.Contains(friendUserId)) return BadRequest("User has already gifted the friend today.");

            if (dbUserDailyActions.GiftedCakeUserIds.Length >= m_ConfigDbContext.AppDefaults.MaxCakeGiftPerDay) return BadRequest("User cannot gift more friends today.");

            dbUserDailyActions.GiftedCakeUserIds = dbUserDailyActions.GiftedCakeUserIds.ToList().Append(friendUserId).ToArray();

            var dbUserHappiness = await m_CoreDbContext.UsersHappiness
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            // create the message for friend for receiving
            var dbUserMessage = MessagesController.CreateCakeMessage(userId, dbUserProfile.Name, friendUserId, sendCakeMessage.CakeType);
            await m_CoreDbContext.UsersMessages.AddAsync(dbUserMessage);

            if (dbUserHappiness == null) return BadRequest("User happiness is not found.");

            dbUserHappiness.Social += 1;
            dbUserProfile.CakeDonated += 1;

            var userStatController = new UserStatsController(this.GetClaimantUserId(), m_CoreDbContext, m_ConfigDbContext);
            await userStatController.AddUserStatValueAsync(UserStatType.CAKE_GIFTED_TOTAL, 1);
            await userStatController.AddUserStatValueAsync(UserStatType.CAKE_GIFTED_, ((int)sendCakeMessage.CakeType).ToString(), 1);

            var questController = new QuestsController(this.GetClaimantUserId(), m_CoreDbContext, m_ConfigDbContext);
            var updatedQuests = await questController.CheckQuestUpdates();
            var newQuests = await questController.CheckNewQuests();

            await m_CoreDbContext.SaveChangesAsync();

            this.AddDataToReturnList(await this.GetStatus());

            if (updatedQuests.Count > 0 || newQuests.Count > 0)
            {
                this.AddDataToReturnList(await this.GetUserQuests());
            }

            return RequestResult("");
        }

        [Authorize]
        [HttpPost("visit", Name = nameof(VisitFriend))]
        public async Task<IActionResult> VisitFriend([FromBody] string friendUserId)
        {
            if (string.IsNullOrEmpty(friendUserId))
            {
                return BadRequest("Friend UserId was invalid.");
            }

            // TODO: This seems to be choking on the friends UserId, which is a valid GUID
            //if (this.IsValidUserId(friendUserId))
            //{
            //    return Forbid("Friend UserId is not valid.");
            //}

            var userId = this.GetClaimantUserId();

            // Check users friend count
            var dbUserProfile = await m_CoreDbContext.UsersProfiles
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            if (dbUserProfile == null)
            {
                return BadRequest("Profile for UserId was not found.");
            }

            var dbUserFriend = await m_CoreDbContext.UsersFriends
                .Where(i => i.UserId == userId && i.FriendUserId == friendUserId)
                .SingleOrDefaultAsync();

            if (dbUserFriend == null) return BadRequest("User is not in friend list.");

            dbUserFriend.LastVisitDate = DateTime.UtcNow;

            // check if the user has already gifted the friend or has reached the max number of gift today
            var dbUserDailyActions = await m_CoreDbContext.UsersDailyActions
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            if (dbUserDailyActions == null) return BadRequest("User daily actions record not found.");
            dbUserDailyActions.Update();

            if (dbUserDailyActions.VisitedUserIds.Contains(friendUserId)) return BadRequest("User has already gifted the friend today.");

            if (dbUserDailyActions.VisitedUserIds.Length >= m_ConfigDbContext.AppDefaults.MaxFriendVisitPerDay) return BadRequest("User cannot gift more friends today.");

            dbUserDailyActions.VisitedUserIds = dbUserDailyActions.VisitedUserIds.ToList().Append(friendUserId).ToArray();

            var dbFriendHappiness = await m_CoreDbContext.UsersHappiness
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            if (dbFriendHappiness != null)
            {
                dbFriendHappiness.Social += 1;
            }

            var userStatController = new UserStatsController(this.GetClaimantUserId(), m_CoreDbContext, m_ConfigDbContext);
            await userStatController.AddUserStatValueAsync(UserStatType.FRIEND_VISIT, 1);

            var questController = new QuestsController(this.GetClaimantUserId(), m_CoreDbContext, m_ConfigDbContext);
            var updatedQuests = await questController.CheckQuestUpdates();
            var newQuests = await questController.CheckNewQuests();

            await m_CoreDbContext.SaveChangesAsync();

            this.AddDataToReturnList(await this.GetStatus());

            if (updatedQuests.Count > 0 || newQuests.Count > 0)
            {
                this.AddDataToReturnList(await this.GetUserQuests());
            }

            return RequestResult("");
        }
    }
}
