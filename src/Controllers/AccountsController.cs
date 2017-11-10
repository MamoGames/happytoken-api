using HappyTokenApi.Data.Core;
using HappyTokenApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using HappyTokenApi.Data.Config;

namespace HappyTokenApi.Controllers
{
    [Route("[controller]")]
    public class AccountsController : DataController
    {
        public AccountsController(CoreDbContext coreDbContext, ConfigDbContext configDbContext) : base(coreDbContext, configDbContext)
        {
        }

        [AllowAnonymous]
        [HttpPost("email", Name = nameof(AuthByEmail))]
        public async Task<IActionResult> AuthByEmail([FromBody] UserEmailLogin userEmailLogin)
        {
            // Is the email data valid?
            if (string.IsNullOrEmpty(userEmailLogin?.Email) || string.IsNullOrEmpty(userEmailLogin.Password))
            {
                return BadRequest("User authentication data was null or empty.");
            }

            // Pull the users data from the DB
            var dbUser = await m_CoreDbContext.Users
                .Where(dbu => dbu.Email == userEmailLogin.Email && dbu.Password == userEmailLogin.Password)
                .SingleOrDefaultAsync();

            if (dbUser != null)
            {
                var response = new UserAuthPair
                {
                    UserId = dbUser.UserId,
                    AuthToken = dbUser.AuthToken
                };

                return RequestResult(response);
            }

            return BadRequest("Could not authenticate user by email.");
        }

        [HttpPost("email/{userId}", Name = nameof(LinkEmail))]
        public async Task<IActionResult> LinkEmail(string userId, [FromBody] UserEmailLogin userEmailLogin)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("UserId was null or empty.");
            }

            if (string.IsNullOrEmpty(userEmailLogin?.Email) || string.IsNullOrEmpty(userEmailLogin.Password))
            {
                return BadRequest("User email data was null or empty.");
            }

            // If the claimant userId matches the request userId, we allow the update
            if (!this.IsClaimantUserId(userId))
            {
                return BadRequest("UserId was invalid for this request.");
            }

            // Pull the users data from the DB
            var dbUser = await m_CoreDbContext.Users
                .Where(dbu => dbu.UserId == userId)
                .SingleOrDefaultAsync();

            if (dbUser != null)
            {
                dbUser.Email = userEmailLogin.Email;
                dbUser.Password = userEmailLogin.Password;

                await m_CoreDbContext.SaveChangesAsync();

                var response = new RequestResult
                {
                    Content = "Successfully updated email.",
                    StatusCode = 0
                };

                return RequestResult(response);
            }

            return BadRequest("Could not link email to user.");
        }

        [Authorize]
        [HttpPost("updatename", Name = nameof(UpdateName))]
        public async Task<IActionResult> UpdateName([FromBody] string name)
        {
            var userId = this.GetClaimantUserId();

            if (!this.IsValidUserId(userId))
            {
                return BadRequest("UserId is invalid.");
            }

            name = name?.Trim();

            //TODO: perform more name check

            if (string.IsNullOrEmpty(name))
            {
                return BadRequest("User name is required.");
            }

            this.AddDataToReturnList(await this.Status());

            // Pull the users data from the DB
            var dbUser = await m_CoreDbContext.UsersProfiles
                .Where(dbu => dbu.UserId == userId)
                .SingleOrDefaultAsync();

            if (dbUser != null)
            {
                dbUser.Name = name;

                await m_CoreDbContext.SaveChangesAsync();

                return RequestResult("Success");
            }



            return BadRequest("Could not update nanme.");
        }
    }
}