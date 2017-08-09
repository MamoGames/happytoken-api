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
    public class UsersController : Controller
    {
        private readonly CoreDbContext m_CoreDbContext;

        public UsersController(CoreDbContext coreDbContext)
        {
            m_CoreDbContext = coreDbContext;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserByDeviceId([FromBody] UserDevice userDevice)
        {
            if (string.IsNullOrEmpty(userDevice?.DeviceId))
            {
                return BadRequest("DeviceId was invalid.");
            }

            // Check if DeviceId exists
            var dbUser = await m_CoreDbContext.Users
                .Where(dbu => dbu.DeviceId == userDevice.DeviceId)
                .SingleOrDefaultAsync();

            // If it does not exist, create a new user
            if (dbUser == null)
            {
                dbUser = new DbUser()
                {
                    UserId = Guid.NewGuid().ToString(),
                    DeviceId = userDevice.DeviceId,
                    AuthToken = Guid.NewGuid().ToString()
                };

                // Add the user
                await m_CoreDbContext.Users.AddAsync(dbUser);

                // Save changes
                await m_CoreDbContext.SaveChangesAsync();

                // Create the user to send back to the client
                var response = new UserAuthPair()
                {
                    UserId = dbUser.UserId,
                    AuthToken = dbUser.AuthToken
                };

                return Ok(response);
            }

            // User with this DeviceId already exists
            return Forbid();
        }

        [Authorize]
        [HttpGet("{userId}", Name = nameof(GetUserById))]
        public async Task<IActionResult> GetUserById(string userId)
        {
            if(!IsCallerUserId(userId))
            {
                return Forbid();
            }

            var dbUser = await m_CoreDbContext.Users
                .Where(dbu => dbu.UserId == userId)
                .SingleOrDefaultAsync();

            if (dbUser == null)
            {
                return NotFound();
            }

            //var response = new User()
            //{
            //    Href = Url.Link(nameof(GetUserById), new { id = dbUser.UserId }),
            //    Method = "GET",
            //    UserId = dbUser.UserId,
            //    Email = dbUser.Email,
            //    Password = dbUser.Password,
            //    DeviceId = dbUser.DeviceId,
            //    SessionToken = dbUser.SessionToken
            //};

            return Ok("dawg");
        }

        //[HttpGet(Name = nameof(GetAllUsers))]
        //public async Task<IActionResult> GetAllUsers()
        //{
        //    var users = (await m_CoreDbContext.Users.ToArrayAsync())
        //        .Select(dbUser => new User
        //        {
        //            Href = Url.Link(nameof(GetUserById), new { id = dbUser.UserId }),
        //            Method = "GET",
        //            UserId = dbUser.UserId,
        //            Email = dbUser.Email,
        //            Password = dbUser.Password,
        //            DeviceId = dbUser.DeviceId,
        //            SessionToken = dbUser.SessionToken
        //        })
        //        .ToArray();

        //    var response = new Collection<User>
        //    {
        //        Href = Url.Link(nameof(GetAllUsers), null),
        //        Relations = new[] { "collection" },
        //        Value = users
        //    };

        //    return Ok(response);
        //}

        /// <summary>
        /// Used to ensure the user requested in the method matches the user in the JWT Claim.
        /// For example; A user can only request their own profile
        /// </summary>
        private bool IsCallerUserId(string userId)
        {
            // Ensure we have both the user and Claims data
            if (string.IsNullOrEmpty(userId) || User.Claims.Count() == 0)
            {
                return false;
            }

            // Grab the UserId from the Claim
            var claimsUserId = User.Claims.First().Value;

            // Ensure the UserId and Claim UserId match
            return !string.IsNullOrEmpty(claimsUserId) && userId == claimsUserId;
        }
    }
}