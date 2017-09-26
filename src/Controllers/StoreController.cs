﻿using HappyTokenApi.Data.Config;
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
using Microsoft.AspNetCore.Mvc.TagHelpers;
using System.Collections.Generic;

namespace HappyTokenApi.Controllers
{
    [Route("[controller]")]
    public class StoreController : Controller
    {
        private readonly CoreDbContext m_CoreDbContext;

        private readonly ConfigDbContext m_ConfigDbContext;

        public StoreController(CoreDbContext coreDbContext, ConfigDbContext configDbContext)
        {
            m_CoreDbContext = coreDbContext;
            m_ConfigDbContext = configDbContext;
        }

        [Authorize]
        [HttpPost("promotions", Name = nameof(BuyPromotion))]
        public async Task<IActionResult> BuyPromotion([FromBody] string promotionCode)
        {
            var userId = this.GetClaimantUserId();

            if (!this.IsValidUserId(userId))
            {
                return BadRequest("UserId is invalid.");
            }

            var promotion = m_ConfigDbContext.Store.Promotions.Find(i => i.Code == promotionCode);

            if (promotion == null)
            {
                return BadRequest("Requested Promotion code is invalid.");
            }

            // Check User has the currency required
            var dbUserWallet = await m_CoreDbContext.UsersWallets
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            if (dbUserWallet == null)
            {
                return BadRequest("Could not find Users Wallet.");
            }

            // Check user has enough to buy Promotion
            var isBuySuccessful = false;
            if (dbUserWallet.Gold >= promotion.Price)
            {
                isBuySuccessful = true;
                dbUserWallet.Gold -= promotion.Price;
            }

            if (!isBuySuccessful)
            {
                return BadRequest("User does not have enough gold to buy this Promotion.");
            }

            // Allocate resources
            dbUserWallet.HappyTokens += promotion.HappyTokens;
            dbUserWallet.Gold += promotion.Gold;
            dbUserWallet.Gems += promotion.Gems;

            // Allocate Avatar Pieces
            if (promotion.AvatarPieces.Count > 0)
            {
                var dbUserAvatars = await m_CoreDbContext.UsersAvatars
                    .Where(i => i.UserId == userId)
                    .ToListAsync();

                if (dbUserAvatars != null)
                {
                    foreach (var avatarPiece in promotion.AvatarPieces)
                    {
                        var avatar = dbUserAvatars.Find(i => i.AvatarType == avatarPiece.AvatarType);

                        if (avatar != null)
                        {
                            avatar.Pieces += avatarPiece.Pieces;
                        }
                    }
                }
            }

            await m_CoreDbContext.SaveChangesAsync();

            // Send the updated Wallet back to the user
            var wallet = (Wallet)dbUserWallet;

            return Ok(wallet);
        }

        [Authorize]
        [HttpPost("resourcemines", Name = nameof(BuyResourceMine))]
        public async Task<IActionResult> BuyResourceMine([FromBody] ResourceMineType resourceMineType)
        {
            var userId = this.GetClaimantUserId();

            if (!this.IsValidUserId(userId))
            {
                return BadRequest("UserId is invalid.");
            }

            var resourceMine = m_ConfigDbContext.Store.ResourceMines.Find(i => i.ResourceMineType == resourceMineType);

            if (resourceMine == null)
            {
                return BadRequest("Requested Resource Mine is invalid.");
            }

            // Check User has the currency required
            var dbUserWallet = await m_CoreDbContext.UsersWallets
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            if (dbUserWallet == null)
            {
                return BadRequest("Could not find Users Wallet.");
            }

            // Check user has enough to buy Resource Mine
            var isSellSuccessful = false;
            switch (resourceMine.ResourceMineType)
            {
                case ResourceMineType.Gold:
                    if (dbUserWallet.Gold >= resourceMine.Price)
                    {
                        isSellSuccessful = true;
                        dbUserWallet.Gold -= resourceMine.Price;
                    }
                    break;
                case ResourceMineType.Gems:
                    if (dbUserWallet.Gold >= resourceMine.Price)
                    {
                        isSellSuccessful = true;
                        dbUserWallet.Gold -= resourceMine.Price;
                    }
                    break;
            }

            if (!isSellSuccessful)
            {
                return BadRequest("User does not have enough gold to buy this Resource Mine.");
            }

            // Check User has the currency required
            var dbUserProfile = await m_CoreDbContext.UsersProfiles
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            // Settle the buy part of the transaction
            switch (resourceMine.ResourceMineType)
            {
                case ResourceMineType.Gold:
                    dbUserProfile.GoldMineDaysRemaining += resourceMine.Days;
                    break;
                case ResourceMineType.Gems:
                    dbUserProfile.GemMineDaysRemaining += resourceMine.Days;
                    break;
            }

            await m_CoreDbContext.SaveChangesAsync();

            // Send the updated Wallet back to the user
            var wallet = (Wallet)dbUserWallet;

            return Ok(wallet);
        }

        [Authorize]
        [HttpPost("currencyspots", Name = nameof(BuyCurrencySpot))]
        public async Task<IActionResult> BuyCurrencySpot([FromBody] StoreCurrencySpot storeCurrencySpot)
        {
            var userId = this.GetClaimantUserId();

            if (!this.IsValidUserId(userId))
            {
                return BadRequest("UserId is invalid.");
            }

            var dbStoreCurrencySpot = m_ConfigDbContext.Store.CurrencySpots.Find(
                i => i.ProductID == storeCurrencySpot.ProductID
                && i.BuyAmount == storeCurrencySpot.BuyAmount);

            if (dbStoreCurrencySpot == null)
            {
                return BadRequest("Requested StoreCurrencySpot is invalid.");
            }

            var dbUserWallet = await m_CoreDbContext.UsersWallets
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            if (dbUserWallet == null)
            {
                return BadRequest("Could not find Users Wallet.");
            }

            // Check user has enough Sell Currency Amount to purchase the Buy Currency Amount
            var isSellSuccessful = false;

            if (!storeCurrencySpot.Cost.PurchaseWith(dbUserWallet))
            {
                return BadRequest("User does not have enough resources.");
            }

            if (!isSellSuccessful)
            {
                return BadRequest("User does not have enough currency to sell to buy this amount of currency.");
            }

            // Settle the buy part of the transaction
            switch (storeCurrencySpot.BuyCurrencyType)
            {
                case CurrencyType.Gems:
                    dbUserWallet.Gems += dbStoreCurrencySpot.BuyAmount;
                    break;
                case CurrencyType.Gold:
                    dbUserWallet.Gold += dbStoreCurrencySpot.BuyAmount;
                    break;
                case CurrencyType.HappyTokens:
                    dbUserWallet.HappyTokens += dbStoreCurrencySpot.BuyAmount;
                    break;
            }

            await m_CoreDbContext.SaveChangesAsync();

            // Send the updated Wallet back to the user
            var wallet = (Wallet)dbUserWallet;

            return Ok(wallet);
        }

        [Authorize]
        [HttpPost("avatars", Name = nameof(BuyAvatar))]
        public async Task<IActionResult> BuyAvatar([FromBody] AvatarType avatarType)
        {
            var userId = this.GetClaimantUserId();

            if (!this.IsValidUserId(userId))
            {
                return BadRequest("UserId is invalid.");
            }

            var dbAvatar = m_ConfigDbContext.Store.Avatars.Find(i => i.AvatarType == avatarType);

            if (dbAvatar == null)
            {
                return BadRequest("AvatarType is invalid.");
            }

            // Check if User owns the Avatar and avatar is 1 - Upgrade level
            var dbUsersAvatars = await m_CoreDbContext.UsersAvatars
                .Where(i => i.UserId == userId)
                .ToListAsync();

            if (dbUsersAvatars.Exists(i => i.AvatarType == avatarType))
            {
                return BadRequest("User already owns this Avatar.");
            }

            // Check User has the currency required
            var dbUserWallet = await m_CoreDbContext.UsersWallets
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            if (dbUserWallet == null)
            {
                return BadRequest("Could not find users wallet.");
            }

			if (!dbAvatar.Cost.PurchaseWith(dbUserWallet, dbUsersAvatars.OfType<UserAvatar>().ToList()))
			{
                return BadRequest("User does not have enough resources for this Avatar.");
			}

            // Create the new UserAvatar
            var dbUserAvatar = new DbUserAvatar()
            {
                UsersAvatarId = Guid.NewGuid().ToString(),
                UserId = userId,
                AvatarType = avatarType,
                Level = 1,
                Pieces = 0
            };

            // Add the happiness gained from Level1 to the users Happiness
            var avatar = m_ConfigDbContext.Avatars.Avatars.Find(i => i.AvatarType == avatarType);

            var happinessType = avatar.HappinessType;
            var happinessAmount = avatar.Levels[0].Happiness;

            var dbUserHappiness = await m_CoreDbContext.UsersHappiness
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            // Update the users Avatars and Happiness accordingly
            dbUserHappiness.Add(happinessType, happinessAmount);

            await m_CoreDbContext.UsersAvatars.AddAsync(dbUserAvatar);

            await m_CoreDbContext.SaveChangesAsync();

            // Send the updated Wallet back to the user
            var wallet = (Wallet)dbUserWallet;

            return Ok(wallet);
        }

        [Authorize]
        [HttpPost("avatarupgrades", Name = nameof(BuyAvatarUpgrade))]
        public async Task<IActionResult> BuyAvatarUpgrade([FromBody] StoreAvatarUpgrade storeAvatarUpgrade)
        {
            var userId = this.GetClaimantUserId();

            if (!this.IsValidUserId(userId))
            {
                return BadRequest("UserId is invalid.");
            }

            if (storeAvatarUpgrade.AvatarType == AvatarType.None)
            {
                return BadRequest("AvatarType is invalid.");
            }

            // Grab the Avatar Upgrades for the AvatarType
            var avatarUpgrades = m_ConfigDbContext.Store.AvatarUpgrades.FindAll(i => i.AvatarType == storeAvatarUpgrade.AvatarType);

            if (avatarUpgrades.Count == 0)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            // Ensure the user has requested a valid level
            var avatarUpgrade = avatarUpgrades.Find(i => i.Level == storeAvatarUpgrade.Level);

            if (avatarUpgrade == null)
            {
                return BadRequest("Requested Level does not exist.");
            }

            // Check if User owns the Avatar and avatar is 1 - Upgrade level
            var dbUserAvatar = await m_CoreDbContext.UsersAvatars
                .Where(i => i.UserId == userId && i.AvatarType == storeAvatarUpgrade.AvatarType)
                .SingleOrDefaultAsync();

            if (dbUserAvatar == null || dbUserAvatar.Level + 1 != storeAvatarUpgrade.Level)
            {
                return BadRequest("Requested AvatarUpgrade is not valid for Users Avatar.");
            }

            // Check User has the currency required
            var dbUserWallet = await m_CoreDbContext.UsersWallets
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            if (!avatarUpgrade.Cost.PurchaseWith(dbUserWallet, new List<UserAvatar> { dbUserAvatar, }))
			{
				return BadRequest("User does not have enough resources for this AvatarUpgrade.");
			}

            // Deduct the currency and upgrade the avatars level
            dbUserAvatar.Level = avatarUpgrade.Level;

            await m_CoreDbContext.SaveChangesAsync();

            // Send the updated Wallet back to the user
            var wallet = (Wallet)dbUserWallet;

            return Ok(wallet);
        }

        [Authorize]
        [HttpPost("buildings", Name = nameof(BuyBuilding))]
        public async Task<IActionResult> BuyBuilding([FromBody] BuildingType buildingType)
        {
            var userId = this.GetClaimantUserId();

            if (!this.IsValidUserId(userId))
            {
                return BadRequest("UserId is invalid.");
            }

            var dbBuilding = m_ConfigDbContext.Store.Buildings.Find(i => i.BuildingType == buildingType);

            if (dbBuilding == null)
            {
                return BadRequest("BuildingType is invalid.");
            }

            // Check if User owns the Avatar and avatar is 1 - Upgrade level
            var dbUsersBuildings = await m_CoreDbContext.UsersBuildings
                .Where(i => i.UserId == userId)
                .ToListAsync();

            if (dbUsersBuildings.Exists(i => i.BuildingType == buildingType))
            {
                return BadRequest("User already owns this Building.");
            }

            // Check User has the currency required
            var dbUserWallet = await m_CoreDbContext.UsersWallets
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            if (dbUserWallet == null)
            {
                return BadRequest("Could not find users wallet.");
            }

			if (!dbBuilding.Cost.PurchaseWith(dbUserWallet))
			{
				return BadRequest("User does not have enough resources for this Building.");
			}

            // Create the new UserAvatar
            var dbUserBuilding = new DbUserBuilding()
            {
                UsersBuildingId = Guid.NewGuid().ToString(),
                UserId = userId,
                BuildingType = buildingType,
                Level = 1,
            };

            // Add the happiness gained from Level1 to the users Happiness
            var building = m_ConfigDbContext.Buildings.Buildings.Find(i => i.BuildingType == buildingType);

            var happinessType = building.HappinessType;
            var happinessAmount = building.Levels[0].Happiness;

            var dbUserHappiness = await m_CoreDbContext.UsersHappiness
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            // Update the users Avatars and Happiness accordingly
            dbUserHappiness.Add(happinessType, happinessAmount);

            await m_CoreDbContext.UsersBuildings.AddAsync(dbUserBuilding);

            await m_CoreDbContext.SaveChangesAsync();

            // Send the updated Wallet back to the user
            var wallet = (Wallet)dbUserWallet;

            return Ok(wallet);
        }

        [Authorize]
        [HttpPost("buildingupgrades", Name = nameof(BuyBuildingUpgrade))]
        public async Task<IActionResult> BuyBuildingUpgrade([FromBody] StoreBuildingUpgrade storeBuildingUpgrade)
        {
            var userId = this.GetClaimantUserId();

            if (!this.IsValidUserId(userId))
            {
                return BadRequest("UserId is invalid.");
            }

            if (storeBuildingUpgrade.BuildingType == BuildingType.None)
            {
                return BadRequest("BuildingType is invalid.");
            }

            // Grab the Avatar Upgrades for the AvatarType
            var buildingUpgrades = m_ConfigDbContext.Store.BuildingUpgrades.FindAll(i => i.BuildingType == storeBuildingUpgrade.BuildingType);

            if (buildingUpgrades.Count == 0)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            // Ensure the user has requested a valid level
            var buildingUpgrade = buildingUpgrades.Find(i => i.Level == storeBuildingUpgrade.Level);

            if (buildingUpgrade == null)
            {
                return BadRequest("Requested Level does not exist.");
            }

            // Check if User owns the Avatar and avatar is 1 - Upgrade level
            var dbUserBuilding = await m_CoreDbContext.UsersBuildings
                .Where(i => i.UserId == userId && i.BuildingType == storeBuildingUpgrade.BuildingType)
                .SingleOrDefaultAsync();

            if (dbUserBuilding == null || dbUserBuilding.Level + 1 != storeBuildingUpgrade.Level)
            {
                return BadRequest("Requested BuildingUpgrade is not valid for Users Building.");
            }

            // Check User has the currency required
            var dbUserWallet = await m_CoreDbContext.UsersWallets
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            if (!buildingUpgrade.Cost.PurchaseWith(dbUserWallet))
            {
                return BadRequest("User does not have enough resources for this BuildingUpgrade.");
            }

            dbUserBuilding.Level = buildingUpgrade.Level;

            await m_CoreDbContext.SaveChangesAsync();

            // Send the updated Wallet back to the user
            var wallet = (Wallet)dbUserWallet;

            return Ok(wallet);
        }
    }
}