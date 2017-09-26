using System.Collections.Generic;

namespace HappyTokenApi.Models
{
    public class Store
    {
        public bool IsO2OOpen { get; set; }

        public List<StorePromotion> Promotions { get; set; }

        public List<ResourceMine> ResourceMines { get; set; }

        public List<StoreCurrencySpot> CurrencySpots { get; set; }

        public List<StoreAvatar> Avatars { get; set; }

        public List<StoreAvatarUpgrade> AvatarUpgrades { get; set; }

        public List<StoreBuilding> Buildings { get; set; }

        public List<StoreBuildingUpgrade> BuildingUpgrades { get; set; }

        public List<StoreO2OProduct> O2OProducts { get; set; }
    }
}
