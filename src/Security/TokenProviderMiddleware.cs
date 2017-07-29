using HappyTokenApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using HappyTokenApi.Data.Core;
using Microsoft.EntityFrameworkCore;

namespace HappyTokenApi.Security
{
    public class TokenProviderMiddleware
    {
        private readonly RequestDelegate m_Next;
        private readonly CoreDbContext m_CoreDbContext;
        private readonly TokenProviderOptions m_Options;
        private readonly JsonSerializerSettings m_SerializerSettings;

        public TokenProviderMiddleware(RequestDelegate next, IOptions<TokenProviderOptions> options, CoreDbContext coreDbContext)
        {
            m_Next = next;
            m_Options = options.Value;
            m_CoreDbContext = coreDbContext;

            ThrowIfInvalidOptions(m_Options);

            m_SerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
        }

        public Task Invoke(HttpContext context)
        {
            // If the request path doesn't match, skip
            if (!context.Request.Path.Equals(m_Options.Path, StringComparison.Ordinal))
            {
                return m_Next(context);
            }

            // Request must be POST with Content-Type: application/x-www-form-urlencoded
            if (!context.Request.Method.Equals("POST") || context.Request.ContentType != "application/json")
            {
                context.Response.StatusCode = 400;
                return context.Response.WriteAsync("Bad request.");
            }

            return GenerateToken(context);
        }

        private async Task GenerateToken(HttpContext context)
        {
            string json;
            using (var streamReader = new StreamReader(context.Request.Body))
            {
                json = streamReader.ReadToEnd();
            }

            var userAuthPair = JsonConvert.DeserializeObject<UserAuthPair>(json);

            // Is the UserAuthPair data valid?
            if (string.IsNullOrEmpty(userAuthPair?.UserId) || string.IsNullOrEmpty(userAuthPair.AuthToken))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid authentication data.");
                return;
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
                    var identity = new ClaimsIdentity(new GenericIdentity(userAuthPair.UserId, "Token"), new Claim[] { });
                }
                else
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid username or password.");
                    return;
                }
            }
            else
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("User not found.");
                return;
            }

            var now = DateTime.UtcNow;

            // Specifically add the jti (nonce), iat (issued timestamp), and sub (subject/user) claims. Other claims can be added here
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userAuthPair.UserId),
                new Claim(JwtRegisteredClaimNames.Jti, await m_Options.NonceGenerator()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUniversalTime().ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Create the JWT and write it to a string
            var jwt = new JwtSecurityToken(
                issuer: m_Options.Issuer,
                audience: m_Options.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(m_Options.Expiration),
                signingCredentials: m_Options.SigningCredentials);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new JsonWebToken
            {
                AccessToken = encodedJwt,
                ExpiresInSecs = (int)m_Options.Expiration.TotalSeconds
            };

            // Serialize and return the response
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response, m_SerializerSettings));
        }

        private static void ThrowIfInvalidOptions(TokenProviderOptions options)
        {
            if (string.IsNullOrEmpty(options.Path))
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.Path));
            }

            if (string.IsNullOrEmpty(options.Issuer))
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.Issuer));
            }

            if (string.IsNullOrEmpty(options.Audience))
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.Audience));
            }

            if (options.Expiration == TimeSpan.Zero)
            {
                throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(TokenProviderOptions.Expiration));
            }

            if (options.SigningCredentials == null)
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.SigningCredentials));
            }

            if (options.NonceGenerator == null)
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.NonceGenerator));
            }
        }
    }
}