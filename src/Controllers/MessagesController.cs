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

namespace HappyTokenApi.Controllers
{
    [Route("[controller]")]
    public class MessagesController : Controller
    {
        private readonly CoreDbContext m_CoreDbContext;

        public MessagesController(CoreDbContext coreDbContext, ConfigDbContext configDbContext)
        {
            m_CoreDbContext = coreDbContext;
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
                .Where(i => i.ToUserId == userId)
                .ToListAsync();


            var result = dbUsersMessages.OfType<UserMessage>().ToList();

            return Ok(result);
        }

        [Authorize]
        [HttpPost("cake", Name = nameof(SendCakeMessage))]
        public async Task<IActionResult> SendCakeMessage([FromBody] UserSendCakeMessage sendCakeMessage)
        {
            var userId = this.GetClaimantUserId();
            var toUserId = sendCakeMessage.ToUserId;
            var cakeType = sendCakeMessage.CakeType;

            if (!this.IsValidUserId(userId))
            {
                return BadRequest("UserId is invalid.");
            }

            var dbUserProfile = await m_CoreDbContext.UsersProfiles
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            if (dbUserProfile == null)
            {
                return BadRequest("Could not find sender user.");
            }

            // Have we already sent a cake to this user 
            var isAlreadySent = await m_CoreDbContext.UsersMessages
                .Where(i => i.ToUserId == toUserId && i.FromUserId == userId && i.MessageType == MessageType.Cake)
                .AnyAsync();

            if (isAlreadySent)
            {
                return BadRequest("Cake already sent to this user!");
            }

            // TODO: Add more logic to decide if player can donate a cake to this user
            //var isCakeAvailable = await m_CoreDbContext.UsersCakes
            //    .Where(i => i.UserId == userId && i.CakeType == cakeType)
            //    .AnyAsync();

            //if (!isCakeAvailable)
            //{
            //    return BadRequest("Sorry, you don't have enough cakes to send to this user.");
            //}

            var dbUserMessage = CreateCakeMessage(userId, dbUserProfile.Name, toUserId, cakeType);

            await m_CoreDbContext.UsersMessages.AddAsync(dbUserMessage);

            await m_CoreDbContext.SaveChangesAsync();

            var result = (UserMessage) dbUserMessage;

            return Ok(result);
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
                return BadRequest("Cannot delete other users messages.");
            }

            if (dbUserMessage.MessageType != MessageType.Cake)
            {
                return BadRequest("This message cannot be used to claim cakes.");
            }

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

            return Ok(result);
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

            if (dbUserMessage.ToUserId != userId)
            {
                return BadRequest("Cannot delete other users messages.");
            }

            m_CoreDbContext.UsersMessages.Remove(dbUserMessage);

            await m_CoreDbContext.SaveChangesAsync();

            return await GetMessages();
        }

        private DbUserMessage CreateCakeMessage(string fromUserId, string fromUserName, string toUserId, CakeType cakeType)
        {
            var userMessage = new DbUserMessage
            {
                UsersMessageId = Guid.NewGuid().ToString(),
                MessageType = MessageType.Cake,
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