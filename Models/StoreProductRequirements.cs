using System;
using System.Collections.Generic;

namespace HappyTokenApi.Models
{
    public class StoreProductRequirements
    {
        public int MinLevel { get; set; }

        public DateTime After { get; set; }

        public DateTime Before { get; set; }

        public List<UserBuilding> BuildingLevels { get; set; }

        /// <summary>
        /// User must has these avatars (level 1 or above)
        /// </summary>
        public List<AvatarType> Avatars { get; set; }

        /// <summary>
        /// 0 means there is no limitation
        /// </summary>
        /// <value>The max purchase count.</value>
        public int MaxPurchaseCount { get; set; }

        /// <summary>
        /// To be used by client side for checking if the criteria is met for the user right now.
        /// </summary>
        /// <returns><c>true</c>, if all criterias are met, <c>false</c> otherwise.</returns>
        /// <param name="profile">User profile</param>
        public bool IsMet(string ProductID, UserLogin userLogin)
        {
            var now = DateTime.UtcNow;

			if (this.After != default(DateTime) && now < this.After) return false;

            if (this.Before != default(DateTime) && now > this.Before) return false;

            if (MinLevel > 0 && (userLogin.Profile.Level < MinLevel)) return false;

            if (this.BuildingLevels != null)
            {
                foreach (var requireBuilding in this.BuildingLevels)
                {
                    var building = userLogin.UserBuildings.Find(i => i.BuildingType == requireBuilding.BuildingType);
                    if (building == null) return false;

                    if (building.Level < requireBuilding.Level) return false;
                }
            }

            if (this.Avatars != null)
            {
                foreach (var avatar in this.Avatars)
                {
                    if (!(userLogin.UserAvatars.Exists(i => i.AvatarType == avatar && i.Level >= 1))) return false;
                }
            }

            if (this.MaxPurchaseCount > 0)
            {
                var purchase = userLogin.UserStorePurchaseRecords.Find(i => i.StoreProductId == ProductID);

                if (purchase != null && purchase.Count >= this.MaxPurchaseCount) return false;
            }

            return true;
        }
    }
}
