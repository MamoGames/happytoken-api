using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace HappyTokenApi.Security
{
    public class TokenIdentityValidation
    {
        public Task<ClaimsIdentity> GetClaimsIdentity(string username, string password)
        {
            Console.WriteLine($"TESTING CLAIM {username}, {password}!");

            // DEMO CODE, DON NOT USE IN PRODUCTION!!!
            if (username == "TEST" && password == "TEST123")
            {
                return Task.FromResult(new ClaimsIdentity(new GenericIdentity(username, "Token"), new Claim[] { }));
            }

            // Account doesn't exist
            return Task.FromResult<ClaimsIdentity>(null);
        }
    }
}
