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
    public class MessagesController : DataController
    {
        public MessagesController(CoreDbContext coreDbContext, ConfigDbContext configDbContext) : base(coreDbContext, configDbContext)
        {
        }

        [Authorize]
        [HttpGet("", Name = nameof(GetMessages))]
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

            return RequestResult(result);
        }

        [Authorize]
        [HttpGet("cake/{userMessageId}", Name = nameof(ClaimCakeMessage))]
        public async Task<IActionResult> ClaimCakeMessage(string userMessageId)
        {
            var userId = this.GetClaimantUserId();

            if (!this.IsValidUserId(userId))
            {
                return BadRequest("UserId is invalid.");
            }

            var dbUserMessage = await m_CoreDbContext.UsersMessages
                .Where(i => i.UsersMessageId == userMessageId)
                .SingleOrDefaultAsync();

            if (dbUserMessage.ToUserId != userId)
            {
                return BadRequest("Cannot calim other users messages.");
            }

            if (dbUserMessage.MessageType != MessageType.GiftCake)
            {
                return BadRequest("This message cannot be used to claim cakes.");
            }

            if (dbUserMessage.IsDeleted)
            {
                return BadRequest("The message is already deleted.");
            }

            //TODO: check inventory for space

            var dbNewCake = new DbUserCake()
            {
                UsersCakeId = Guid.NewGuid().ToString(),
                UserId = userId,
                CakeType = dbUserMessage.PinkyCakeType,
                Value = dbUserMessage.PinkyCakes,
                IsBaked = true,
                BakedDate = DateTime.UtcNow,
            };

            await m_CoreDbContext.UsersCakes.AddAsync(dbNewCake);

            await m_CoreDbContext.SaveChangesAsync();

            await DeleteMessage(userMessageId);

            var dbUserCakes = await m_CoreDbContext.UsersCakes
                .Where(i => i.UserId == userId)
                .ToListAsync();

            var result = dbUserCakes.OfType<UserCake>().ToList();

            return RequestResult(result);
        }

        [Authorize]
        [HttpGet("rewards/{userMessageId}", Name = nameof(ClaimMessageRewards))]
        public async Task<IActionResult> ClaimMessageRewards(string userMessageId)
        {
            var userId = this.GetClaimantUserId();

            if (!this.IsValidUserId(userId))
            {
                return BadRequest("UserId is invalid.");
            }

            var dbUserMessage = await m_CoreDbContext.UsersMessages
                .Where(i => i.UsersMessageId == userMessageId)
                .SingleOrDefaultAsync();

            if (dbUserMessage.ToUserId != userId)
            {
                return BadRequest("Cannot claim other users messages.");
            }

            if (dbUserMessage.MessageType != MessageType.QuestRewards)
            {
                return BadRequest("This message cannot be used to claim rewards.");
            }

            if (dbUserMessage.IsDeleted)
            {
                return BadRequest("The message is already deleted.");
            }

            var dbUserWallet = await m_CoreDbContext.UsersWallets
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            if (dbUserWallet == null)
            {
                return BadRequest("Could not find Users Wallet.");
            }

            if (dbUserMessage.Gems > 0 || dbUserMessage.Gold > 0 || dbUserMessage.HappyTokens > 0)
            {
                dbUserWallet.Gems += dbUserMessage.Gems;
                dbUserWallet.Gold += dbUserMessage.Gold;
                dbUserWallet.HappyTokens += dbUserMessage.HappyTokens;
            }

            if (dbUserMessage.Xp > 0)
            {
                var dbUserProfile = await m_CoreDbContext.UsersProfiles
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

                if (dbUserProfile == null)
                {
                    return BadRequest("Could not find user profile.");
                }

                dbUserProfile.AddXp(dbUserMessage.Xp);
            }

            //TODO: check inventory and item rewards


            await m_CoreDbContext.SaveChangesAsync();

            await DeleteMessage(userMessageId);

            // Send the updated Wallet back to the user
            var wallet = (Wallet)dbUserWallet;

            return RequestResult(wallet);
        }


        [Authorize]
        [HttpPost("delete", Name = nameof(DeleteMessage))]
        public async Task<IActionResult> DeleteMessage([FromBody] string userMessageId)
        {
            var userId = this.GetClaimantUserId();

            if (!this.IsValidUserId(userId))
            {
                return BadRequest("UserId is invalid.");
            }

            var dbUserMessage = await m_CoreDbContext.UsersMessages
                .Where(i => i.UsersMessageId == userMessageId)
                .SingleOrDefaultAsync();

            // can only delete messageds that target the user
            if (dbUserMessage.ToUserId != userId)
            {
                return BadRequest("Cannot delete other users messages.");
            }

            dbUserMessage.IsDeleted = true;

            await m_CoreDbContext.SaveChangesAsync();

            return await GetMessages();
        }

        [Authorize]
        [HttpPost("read", Name = nameof(ReadMessage))]
        public async Task<IActionResult> ReadMessage([FromBody] string userMessageId)
        {
            var userId = this.GetClaimantUserId();

            if (!this.IsValidUserId(userId))
            {
                return BadRequest("UserId is invalid.");
            }

            // read status is stored on a individual entity
            var dbUserMessageStatus = await m_CoreDbContext.UsersMessagesStatus.Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            if (dbUserMessageStatus == null)
            {
                // create the record for storing the data
                dbUserMessageStatus = new DbUserMessagesStatus
                {
                    UsersMessageStatusId = Guid.NewGuid().ToString(),
                    UserId = userId,
                };

                await m_CoreDbContext.UsersMessagesStatus.AddAsync(dbUserMessageStatus);
            }

            if (!dbUserMessageStatus.ReadMessageIds.Contains(userMessageId))
            {
                dbUserMessageStatus.ReadMessageIds = dbUserMessageStatus.ReadMessageIds.ToList().Append(userMessageId).ToArray();
            }

            await m_CoreDbContext.SaveChangesAsync();

            return await GetMessages();
        }

        public static DbUserMessage CreateCakeMessage(string fromUserId, string fromUserName, string toUserId, CakeType cakeType)
        {
            var userMessage = new DbUserMessage
            {
                UsersMessageId = Guid.NewGuid().ToString(),
                MessageType = MessageType.GiftCake,
                FromUserId = fromUserId,
                ToUserId = toUserId,
                PinkyCakeType = cakeType,
                PinkyCakes = 1,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                Subject = "Pinky Cake Recieved",
                Preview = $"You have a new Pinky Cake from {fromUserName}!",
                Body = $"You have received a new Pinky Cake from {fromUserName}.\nTap Claim to get your free Pinky Cake.",
                Image = "PinkyCakeSprite"
            };

            return userMessage;
        }
    }
}