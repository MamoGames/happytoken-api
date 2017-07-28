using System;
using HappyTokenApi.Data.Core;
using HappyTokenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using HappyTokenApi.Data.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;

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
        public async Task<IActionResult> CreateUserByDeviceId([FromBody] string deviceId)
        {
            // Check if DeviceId exists
            var dbUser = await m_CoreDbContext.Users
                .Where(dbu => dbu.DeviceId == deviceId)
                .SingleOrDefaultAsync();

            // If it does not exist, create a new user
            if (dbUser == null)
            {
                dbUser = new DbUser()
                {
                    UserId = Guid.NewGuid().ToString(),
                    DeviceId = deviceId,
                    Email = "",
                    Password = "",
                    SessionToken = Guid.NewGuid().ToString()
                };

                // Add the user
                await m_CoreDbContext.Users.AddAsync(dbUser);

                // Save changes
                await m_CoreDbContext.SaveChangesAsync();

                // Create the user to send back to the client
                var response = new User()
                {
                    Href = Url.Link(nameof(GetUserById), new { id = dbUser.UserId }),
                    Method = "GET",
                    UserId = dbUser.UserId,
                    Email = dbUser.Email,
                    Password = dbUser.Password,
                    DeviceId = dbUser.DeviceId,
                    SessionToken = dbUser.SessionToken
                };

                return Ok(response);
            }

            // USer with this DeviceId already exists
            return Forbid();
        }

        [Authorize]
        [HttpGet("{userId}", Name = nameof(GetUserById))]
        public async Task<IActionResult> GetUserById(string userId)
        {
            var dbUser = await m_CoreDbContext.Users
                .Where(dbu => dbu.UserId == userId)
                .SingleOrDefaultAsync();

            if (dbUser == null)
            {
                return NotFound();
            }

            var response = new User()
            {
                Href = Url.Link(nameof(GetUserById), new { id = dbUser.UserId }),
                Method = "GET",
                UserId = dbUser.UserId,
                Email = dbUser.Email,
                Password = dbUser.Password,
                DeviceId = dbUser.DeviceId,
                SessionToken = dbUser.SessionToken
            };

            return Ok(response);
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
    }
}