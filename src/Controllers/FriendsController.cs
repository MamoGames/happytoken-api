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
    public class FriendsController : Controller
    {
        private readonly CoreDbContext m_CoreDbContext;
        private readonly ConfigDbContext m_ConfigDbContext;

        public FriendsController(CoreDbContext coreDbContext, ConfigDbContext configDbContext)
        {
            m_CoreDbContext = coreDbContext;
            m_ConfigDbContext = configDbContext;
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
                var friendInfo = new FriendInfo
                {
                    UserId = userId,
                    FriendUserId = profile.UserId,
                    Name = profile.Name,
                    LastSeenDate = profile.LastSeenDate,
                    LastVisitDate = DateTime.MinValue,
                    Level = profile.Level,
                    Happiness = new Happiness() // Do we want Happiness?
                };

                friends.Add(friendInfo);
            }

            return Ok(friends);
        }

        [Authorize]
        [HttpGet("", Name = nameof(GetFriends))]
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
                return Ok(new List<FriendInfo>());
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
                }

                var happiness = dbUserHappiness.Find(i => i.UserId == dbUserFriend.FriendUserId);

                if (happiness != null)
                {
                    friend.Happiness = happiness;
                }

                friends.Add(friend);
            }

            return Ok(friends);
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

            var dbUserFriends = await m_CoreDbContext.UsersFriends
                .Where(i => i.UserId == userId)
                .ToListAsync();

            if (dbUserFriends != null)
            {
                // Check user is not already friend in either direction
                if (dbUserFriends.Any(i => i.UsersFriendId == friendUserId))
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

            await m_CoreDbContext.SaveChangesAsync();

            // Grab the entire (updated) list of friends, and return to the user
            return await GetFriends();
        }
    }
}
