using System.Collections.Generic;

namespace HappyTokenApi.Models
{
    public class Store
    {
        public List<StorePromotion> Promotions { get; set; }

        public List<ResourceMine> ResourceMines { get; set; }

        public List<StoreCurrencySpot> CurrencySpots { get; set; }

        public List<StoreAvatar> Avatars { get; set; }

        public List<StoreAvatarUpgrade> AvatarUpgrades { get; set; }

        public List<StoreBuilding> Buildings { get; set; }

        public List<StoreBuildingUpgrade> BuildingUpgrades { get; set; }
    }
}
