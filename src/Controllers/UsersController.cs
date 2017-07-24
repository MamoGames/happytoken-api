using HappyTokenApi.Models;
using HappyTokenApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTokenApi.Controllers
{
    [Route("[controller]")]
    public class UsersController : Controller
    {
        private readonly ApiDbContext m_ApiDbContext;

        public UsersController(ApiDbContext apiDbContext)
        {
            m_ApiDbContext = apiDbContext;
        }

        [HttpGet("{id:string}", Name = nameof(GetUserById))]
        public async Task<IActionResult> GetUserById(string id)
        {
            var dbUser = await m_ApiDbContext.Users
                .Where(dbu => dbu.Id == id)
                .SingleOrDefaultAsync();

            if (dbUser == null)
            {
                return NotFound();
            }

            var response = new User()
            {
                Href = Url.Link(nameof(GetUserById), new { id = dbUser.Id }),
                Method = "GET",
                FirstName = dbUser.FirstName,
                LastName = dbUser.LastName
            };

            return Ok(response);
        }

        [HttpGet(Name = nameof(GetAllUsers))]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = (await m_ApiDbContext.Users.ToArrayAsync())
                .Select(dbUser => new User
                {
                    Href = Url.Link(nameof(GetUserById), new { id = dbUser.Id }),
                    Method = "GET",
                    FirstName = dbUser.FirstName,
                    LastName = dbUser.LastName
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