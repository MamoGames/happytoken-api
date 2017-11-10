using HappyTokenApi.Data.Config;
using HappyTokenApi.Data.Core;
using HappyTokenApi.Data.Core.Entities;
using HappyTokenApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HappyTokenApi.Controllers
{
    [Route("[controller]")]
    public class TownController : DataController
    {
        public TownController(CoreDbContext coreDbContext, ConfigDbContext configDbContext) : base(coreDbContext, configDbContext)
        {
        }

        [Authorize]
        [HttpPost("buildingsupdate", Name = nameof(UpdateUserBuildings))]
        public async Task<IActionResult> UpdateUserBuildings([FromBody] List<UserBuilding> userBuildings)
        {
            var userId = this.GetClaimantUserId();

            if (!this.IsValidUserId(userId))
            {
                return BadRequest("UserId is invalid.");
            }

            var dbUserBuildings = await m_CoreDbContext.UsersBuildings
                .Where(i => i.UserId == userId)
                .ToListAsync();

            foreach (var userBuilding in userBuildings)
            {
                var dbUserBuilding = dbUserBuildings.Find(i => i.BuildingType == userBuilding.BuildingType);

                if (dbUserBuilding != null)
                {
                    // update info for the building, only position for now
                    dbUserBuilding.Position = userBuilding.Position;
                }
            }

            await m_CoreDbContext.SaveChangesAsync();

            // Send the updated buildings back to the user
            return RequestResult(dbUserBuildings.OfType<UserBuilding>().ToList());
        }
    }
}