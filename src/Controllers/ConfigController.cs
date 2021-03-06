﻿using HappyTokenApi.Data.Config;
using HappyTokenApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using HappyTokenApi.Data.Core;

namespace HappyTokenApi.Controllers
{
    [Route("[controller]")]
    public class ConfigController : DataController
    {
        public ConfigController(CoreDbContext coreDbContext, ConfigDbContext configDbContext) : base(coreDbContext, configDbContext)
        {
        }

        [Authorize]
        [HttpGet(Name = nameof(GetClientConfig))]
        public IActionResult GetClientConfig()
        {
            // Grab relevant config data for client app config
            var appConfig = new AppConfig
            {
                ServerDateTime = DateTime.UtcNow,
                AppDefaults = m_ConfigDbContext.AppDefaults,
                Avatars = m_ConfigDbContext.Avatars.Avatars,
                Buildings = m_ConfigDbContext.Buildings.Buildings,
                Cakes = m_ConfigDbContext.Cakes.Cakes,
                Store = m_ConfigDbContext.Store,
            };

            return RequestResult(appConfig);
        }

        [AllowAnonymous]
        [HttpGet("refresh", Name = nameof(RefreshConfig))]
        public IActionResult RefreshConfig()
        {
            m_ConfigDbContext.RefreshConfig();

            return RequestResult($"Config data refreshed at {DateTime.UtcNow}");
        }
    }
}