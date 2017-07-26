using HappyTokenApi.Data.Core;
using HappyTokenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTokenApi.Controllers
{
    [Route("[controller]")]
    public class UsersController : Controller
    {
        private readonly CoreDbContext m_ApiDbContext;

        public UsersController(CoreDbContext apiDbContext)
        {
            m_ApiDbContext = apiDbContext;
        }

        [HttpGet("{userId}", Name = nameof(GetUserById))]
        public async Task<IActionResult> GetUserById(string userId)
        {
            var dbUser = await m_ApiDbContext.Users
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

        [HttpGet(Name = nameof(GetAllUsers))]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = (await m_ApiDbContext.Users.ToArrayAsync())
                .Select(dbUser => new User
                {
                    Href = Url.Link(nameof(GetUserById), new { id = dbUser.UserId }),
                    Method = "GET",
                    UserId = dbUser.UserId,
                    Email = dbUser.Email,
                    Password = dbUser.Password,
                    DeviceId = dbUser.DeviceId,
                    SessionToken = dbUser.SessionToken
                })
                .ToArray();

            var response = new Collection<User>
            {
                Href = Url.Link(nameof(GetAllUsers), null),
                Relations = new[] { "collection" },
                Value = users
            };

            return Ok(response);
        }

    }
}