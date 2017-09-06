using HappyTokenApi.Data.Core;
using HappyTokenApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace HappyTokenApi.Controllers
{
    [Route("[controller]")]
    public class TokenController : Controller
    {
        private readonly TokenSettings m_TokenSettings;
        private readonly CoreDbContext m_CoreDbContext;

        public TokenController(IOptions<TokenSettings> options, CoreDbContext coreDbContext)
        {
            m_TokenSettings = options.Value;
            m_CoreDbContext = coreDbContext;
        }

        [HttpPost("")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserAuthPair userAuthPair)
        {
            // Is the UserAuthPair data valid?
            if (string.IsNullOrEmpty(userAuthPair?.UserId) || string.IsNullOrEmpty(userAuthPair.AuthToken))
            {
                return BadRequest("User authentication data was null or empty.");
            }

            // Pull the users data from the DB
            var dbUser = await m_CoreDbContext.Users
                .Where(dbu => dbu.UserId == userAuthPair.UserId)
                .SingleOrDefaultAsync();

            // Check if the users supplied and stored AuthToken matches
            if (dbUser != null)
            {
                if (dbUser.AuthToken == userAuthPair.AuthToken)
                {
                    var claims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Jti, dbUser.UserId), // TODO: Generate a unique token for each request
                        new Claim(JwtRegisteredClaimNames.Sub, dbUser.UserId)
                    };

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(m_TokenSettings.SecretKey));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var issuer = m_TokenSettings.Issuer;
                    var audience = m_TokenSettings.Audience;

                    var expires = DateTime.Now.AddMinutes(30);

                    var token = new JwtSecurityToken(issuer, audience, claims, expires: expires, signingCredentials: creds);

                    var response = new JsonWebToken
                    {
                        AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                        ExpiresInSecs = 30
                    };

                    return Ok(response);
                }
            }

            return BadRequest("Could not create token.");
        }
    }
}