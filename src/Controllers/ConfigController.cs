using HappyTokenApi.Data.Config;
using HappyTokenApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HappyTokenApi.Controllers
{
    [Route("[controller]")]
    public class ConfigController : Controller
    {
        private readonly ConfigDbContext m_ConfigDbContext;

        public ConfigController(ConfigDbContext configDbContext)
        {
            m_ConfigDbContext = configDbContext;
        }

        [Authorize]
        [HttpGet(Name = nameof(GetClientConfig))]
        public async Task<IActionResult> GetClientConfig()
        {
            // Grab relevant config data for client app config
            var appConfig = new AppConfig
            {
                AppDefaults = m_ConfigDbContext.AppDefaults,
                Avatars = m_ConfigDbContext.Avatars.Avatars,
                Buildings = m_ConfigDbContext.Buildings.Buildings,
                Cakes = m_ConfigDbContext.Cakes.Cakes
            };

            return Ok(appConfig);
        }
    }
}