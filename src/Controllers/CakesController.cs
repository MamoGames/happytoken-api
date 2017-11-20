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
    public class CakesController : DataController
    {
        public CakesController(CoreDbContext coreDbContext, ConfigDbContext configDbContext) : base(coreDbContext, configDbContext)
        {
        }

        [Authorize]
        [HttpPost("bake", Name = nameof(BakeCake))]
        public async Task<IActionResult> BakeCake([FromBody] CakeType cakeType)
        {
            var userId = this.GetClaimantUserId();

            if (!this.IsValidUserId(userId))
            {
                return BadRequest("UserId is invalid.");
            }

            // get the cake config
            var cake = m_ConfigDbContext.Cakes.Cakes.Find(i => i.CakeType == cakeType);

            if (cake == null) return BadRequest("Cake not found.");

            var dbUserBakingCakes = await m_CoreDbContext.UsersCakes
                .Where(i => i.UserId == userId && i.IsBaked == false)
                .ToListAsync();

            var dbUserBakingCake = dbUserBakingCakes.Find(i => i.CakeType == cakeType);

            // cannot bake the same cake more than one at once
            if (dbUserBakingCake != null)
            {
                return BadRequest("The cake is already baking.");
            }

            // TODO: check max concurrenct bake


            // check cost
            var dbUserWallet = await m_CoreDbContext.UsersWallets
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            if (dbUserWallet == null) return BadRequest("User wallet not found.");

            if (dbUserWallet.Gold < cake.Gold) return BadRequest("User dont have enough gold to bake the cake.");

            dbUserWallet.Gold -= cake.Gold;


            // add cake
            var dbNewCake = new DbUserCake()
            {
                UsersCakeId = Guid.NewGuid().ToString(),
                UserId = userId,
                CakeType = cake.CakeType,
                IsBaked = false,
                BakedDate = DateTime.UtcNow,
                Value = cake.Value,          // TODO: change cake value base on criterias
            };

            await m_CoreDbContext.UsersCakes.AddAsync(dbNewCake);

            await m_CoreDbContext.SaveChangesAsync();

            this.AddDataToReturnList(await this.GetStatus());

            var newCake = (UserCake)dbNewCake;

            return RequestResult(newCake);
        }

        [Authorize]
        [HttpPost("finishbake", Name = nameof(FinishBakeCake))]
        public async Task<IActionResult> FinishBakeCake([FromBody] CakeType cakeType)
        {
            var userId = this.GetClaimantUserId();

            if (!this.IsValidUserId(userId))
            {
                return BadRequest("UserId is invalid.");
            }

            // get the cake config
            var cake = m_ConfigDbContext.Cakes.Cakes.Find(i => i.CakeType == cakeType);

            if (cake == null) return BadRequest("Cake not found.");

            var dbUserBakingCake = await m_CoreDbContext.UsersCakes
                .Where(i => i.UserId == userId && i.IsBaked == false && i.CakeType == cakeType)
                .SingleOrDefaultAsync();

            if (dbUserBakingCake == null)
            {
                return BadRequest("Baking cake not found.");
            }

            var dbUserInventoryCakes = await m_CoreDbContext.UsersCakes
                .Where(i => i.UserId == userId && i.IsBaked == true && i.CakeType == cakeType)
                .ToListAsync();

            // TODO: check max inventory
            if (dbUserInventoryCakes.Count >= m_ConfigDbContext.AppDefaults.MaxCakeCount) return BadRequest("No inventory space for new cake.");

            // check cake baking status
            if ((dbUserBakingCake.BakedDate + new TimeSpan(0, cake.BakeTimeMins, 0)) > DateTime.UtcNow) return BadRequest("The cake is not ready yet.");

            // change cake status
            dbUserBakingCake.IsBaked = true;

            var userStatController = new UserStatsController(this.GetClaimantUserId(), m_CoreDbContext, m_ConfigDbContext);
            await userStatController.AddUserStatValueAsync(UserStatType.CAKE_BAKED_TOTAL, 1);
            await userStatController.AddUserStatValueAsync(UserStatType.CAKE_BAKED_, ((int)cakeType).ToString(), 1);

            var questController = new QuestsController(this.GetClaimantUserId(), m_CoreDbContext, m_ConfigDbContext);
            var updatedQuests = await questController.CheckQuestUpdates(userStatController.UpdatedUserStatNames);
            var newQuests = await questController.CheckNewQuests();

            await m_CoreDbContext.SaveChangesAsync();

            this.AddDataToReturnList(await this.GetStatus());

            if (updatedQuests.Count > 0 || newQuests.Count > 0)
            {
                this.AddDataToReturnList(await this.GetUserQuests());
            }

            var bakingCake = (UserCake)dbUserBakingCake;

            return RequestResult(bakingCake);
        }
    }
}