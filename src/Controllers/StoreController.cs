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

		/// <summary>
		/// Add a purchase record for the product ID with the product type
		/// </summary>
		/// <param name="productId">Product identifier.</param>
		protected async Task AddStoreProductPurchaseRecord(string productId)
        {
            var userId = this.GetClaimantUserId();

			var dbPurchaseRecord = await m_CoreDbContext.UsersStorePurchaseRecords
				.Where(i => i.UserId == userId && i.StoreProductId == productId)
				.SingleOrDefaultAsync();

            if (dbPurchaseRecord == null)
            {
                dbPurchaseRecord = new DbUserStorePurchaseRecord()
                {
                    UsersStorePurchaseRecordId = Guid.NewGuid().ToString(),
                    UserId = userId,
                    StoreProductId = productId,
                    LastPurchase = DateTime.UtcNow,
                    Count = 1
                };

                await m_CoreDbContext.UsersStorePurchaseRecords.AddAsync(dbPurchaseRecord);
            }
            else
            {
                dbPurchaseRecord.Count += 1;
                dbPurchaseRecord.LastPurchase = DateTime.UtcNow;
            }
        }

        [Authorize]
        [HttpPost("promotions", Name = nameof(BuyPromotion))]
        public async Task<IActionResult> BuyPromotion([FromBody] string promotionId)
        {
            var userId = this.GetClaimantUserId();

            if (!this.IsValidUserId(userId))
            {
                return BadRequest("UserId is invalid.");
            }

            var promotion = m_ConfigDbContext.Store.Promotions.Find(i => i.PromotionId == promotionId);

            if (promotion == null)
            {
                return BadRequest("Requested Promotion code is invalid.");
            }

            // TODO: validate item

            // Check User has the currency required
            var dbUserWallet = await m_CoreDbContext.UsersWallets
                .Where(i => i.UserId == userId)
                .SingleOrDefaultAsync();

            if (dbUserWallet == null)
            {
                return BadRequest("Could not find Users Wallet.");
            }

            var promotedProduct = promotion.GetPromotedStoreProduct(m_ConfigDbContext.Store);

            if (promotedProduct == null) return BadRequest("Promoted store product not found");

            if (! promotion.Cost.PurchaseWith(dbUserWallet, null)) return BadRequest("User does not have enough resources to buy this item.");

            //TODO: handle each product type purchase

            // Allocate Avatar Pieces
            switch (promotion.StoreProductType)
            {
				case StoreProductType.Avatar:
                    var resultBuyAvatar = await this.ReceiveProductForAvatarPurchase(userId, (promotedProduct as StoreAvatar).AvatarType);
                    if (resultBuyAvatar != null) return resultBuyAvatar;

                    break;
				case StoreProductType.AvatarUpgrade:
                    return BadRequest("Not supported.");
				case StoreProductType.Building:
					var resultBuyBuilding = await this.ReceiveProductForBuildingPurchase(userId, (promotedProduct as StoreBuilding).BuildingType);
					if (resultBuyBuilding != null) return resultBuyBuilding;

                    break;
				case StoreProductType.BuildingUpgrade:
                    return BadRequest("Not supported.");
				case StoreProductType.CurrencySpot:
                    var resultBuyCurrencySpot = await this.ReceiveProductForCurrencySpot(userId, dbUserWallet, (promotedProduct as StoreCurrencySpot));
                    if (resultBuyCurrencySpot != null) return resultBuyCurrencySpot;

                    break;
                case StoreProductType.ResourceMine:
                    var resultBuyResourceMine = await this.ReceiveProductForResourceMine(userId, (promotedProduct as ResourceMine));
					if (resultBuyResourceMine != null) return resultBuyResourceMine;

                    break;
				case StoreProductType.O2OProduct:
                    var result = await this.ReceiveProductForO2OPurchase(userId, (promotedProduct as StoreO2OProduct));
					if (result != null) return result;

					break;
                default:
                    return BadRequest("Invalid product type");
            }

            await this.AddStoreProductPurchaseRecord(promotion.PromotedProductId);

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

			if (!resourceMine.Cost.PurchaseWith(dbUserWallet, null))
			{
				return BadRequest("User does not have enough resources for this Resource Mine.");
			}

            var result = await this.ReceiveProductForResourceMine(userId, resourceMine);
            if (result != null) return result;

            await this.AddStoreProductPurchaseRecord(resourceMine.ProductId);

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
                i => i.ProductId == storeCurrencySpot.ProductId);

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

            if (!storeCurrencySpot.Cost.PurchaseWith(dbUserWallet))
            {
                return BadRequest("User does not have enough resources.");
            }

            var result = await this.ReceiveProductForCurrencySpot(userId, dbUserWallet, storeCurrencySpot);
            if (result != null) return result;

            await this.AddStoreProductPurchaseRecord(storeCurrencySpot.ProductId);

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

            // TODO: handle getting pieces before buying the avatar

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

            var result = await this.ReceiveProductForAvatarPurchase(userId, avatarType);
            if (result != null) return result;

            await this.AddStoreProductPurchaseRecord(dbAvatar.ProductId);

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

            var result = await this.ReceiveProductForBuildingPurchase(userId, buildingType);
            if (result != null) return null;

            await this.AddStoreProductPurchaseRecord(dbBuilding.ProductId);

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

		[Authorize]
		[HttpPost("o2oproducts", Name = nameof(BuyO2OProduct))]
		public async Task<IActionResult> BuyO2OProduct([FromBody] StoreO2OProduct storeO2OProduct)
		{
			var userId = this.GetClaimantUserId();

			if (!this.IsValidUserId(userId))
			{
				return BadRequest("UserId is invalid.");
			}

			var dbStoreO2OProduct = m_ConfigDbContext.Store.O2OProducts.Find(
				i => i.ProductId == storeO2OProduct.ProductId);

			if (dbStoreO2OProduct == null)
			{
				return BadRequest("Requested O2OProduct is invalid.");
			}

			var dbUserWallet = await m_CoreDbContext.UsersWallets
				.Where(i => i.UserId == userId)
				.SingleOrDefaultAsync();

			if (dbUserWallet == null)
			{
				return BadRequest("Could not find Users Wallet.");
			}

			if (!storeO2OProduct.Cost.PurchaseWith(dbUserWallet))
			{
				return BadRequest("User does not have enough resources.");
			}

            var result = await this.ReceiveProductForO2OPurchase(userId, storeO2OProduct);
            if (result != null) return result;
			
            await this.AddStoreProductPurchaseRecord(storeO2OProduct.ProductId);

			await m_CoreDbContext.SaveChangesAsync();

			// Send the updated Wallet back to the user
			var wallet = (Wallet)dbUserWallet;

			return Ok(wallet);
		}

        protected async Task<IActionResult> ReceiveProductForAvatarPurchase(string userId, AvatarType avatarType)
		{
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

			return null;
		}

		protected async Task<IActionResult> ReceiveProductForResourceMine(string userId, ResourceMine resourceMine)
		{
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
				default:
					return BadRequest("Unsupported ResourceMineType");
			}

			return null;
		}

		protected async Task<IActionResult> ReceiveProductForCurrencySpot(string userId, DbUserWallet dbUserWallet, StoreCurrencySpot storeCurrencySpot)
		{
			// Settle the buy part of the transaction
			if (!storeCurrencySpot.Wallet.IsEmpty)
			{
				dbUserWallet.Gems += storeCurrencySpot.Wallet.Gems;
				dbUserWallet.Gold += storeCurrencySpot.Wallet.Gold;
				dbUserWallet.HappyTokens += storeCurrencySpot.Wallet.HappyTokens;
			}

			if (storeCurrencySpot.AvatarPieces != null)
			{
				var dbUsersAvatars = await m_CoreDbContext.UsersAvatars
				.Where(i => i.UserId == userId)
				.ToListAsync();

				foreach (var piece in storeCurrencySpot.AvatarPieces)
				{
					var userAvatar = dbUsersAvatars.Find(i => i.AvatarType == piece.AvatarType);

					if (userAvatar == null)
					{
						// Create the new UserAvatar
						var dbUserAvatar = new DbUserAvatar()
						{
							UsersAvatarId = Guid.NewGuid().ToString(),
							UserId = userId,
							AvatarType = piece.AvatarType,
							Level = 0,
							Pieces = piece.Pieces
						};

						await m_CoreDbContext.UsersAvatars.AddAsync(dbUserAvatar);
					}
					else
					{
						userAvatar.Pieces += piece.Pieces;
					}
				}
			}

			return null;
		}

		protected async Task<IActionResult> ReceiveProductForBuildingPurchase(string userId, BuildingType buildingType)
		{
			// Check if User owns the Building
			var dbUsersBuildings = await m_CoreDbContext.UsersBuildings
				.Where(i => i.UserId == userId)
				.ToListAsync();

			if (dbUsersBuildings.Exists(i => i.BuildingType == buildingType))
			{
				return BadRequest("User already owns this Building.");
			}

			// Create the new UserBuilding
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

			return null;
		}

		protected async Task<IActionResult> ReceiveProductForO2OPurchase(string userId, StoreO2OProduct storeO2OProduct)
		{
            //TODO: handle O2O product purchase
            await Task.FromResult(0);

			return null;
		}
	}
}