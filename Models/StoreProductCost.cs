using System.Collections.Generic;

namespace HappyTokenApi.Models
{
    public class StoreProductCost
    {
        public Wallet Wallet { get; set; }

        public AvatarPiece AvatarPiece { get; set; }

        public string IAPProductId { get; set; }

        public string IAPReferencePrice { get; set; }

        public CurrencyType CurrencyType 
        {
            get
            {
                if (Wallet != null)
                {
                    if (Wallet.Gems > 0) return CurrencyType.Gems;

                    if (Wallet.Gold > 0) return CurrencyType.Gold;

                    if (Wallet.HappyTokens > 0) return CurrencyType.HappyTokens;
                }

                return CurrencyType.None;
            }  
        }

        public bool IsIAP => !string.IsNullOrEmpty(this.IAPProductId) && !string.IsNullOrEmpty(this.IAPReferencePrice);

        public bool IsValid
        {
            get
            {
                // perform extra checking on the values to ensure the item is setup correctly, to prevent careless setup that could damage the economy. 
                if (this.IsIAP)
                {
                    // IAP product, should not cost anything else

                    if (this.Wallet != null && !this.Wallet.IsEmpty) return false;

                    if (this.AvatarPiece != null && this.AvatarPiece.Pieces != 0) return false;

                    return true;
                }
                else
                {
                    // IAP should left empty (to prevent careless missing values)

                    if (!string.IsNullOrEmpty(this.IAPProductId) || !string.IsNullOrEmpty(this.IAPReferencePrice)) return false;

                    if (this.AvatarPiece != null && this.AvatarPiece.Pieces < 0) return false;

                    if (this.Wallet != null) 
                    {
                        if (this.Wallet.Gems < 0 || this.Wallet.HappyTokens < 0 || this.Wallet.Gold < 0) return false;

						//only at most one of the fields in wallet should be > 0
						int total = this.Wallet.Gems + this.Wallet.HappyTokens + this.Wallet.Gold;
						if (total > this.Wallet.Gems && total > this.Wallet.HappyTokens && total > this.Wallet.Gold) return false;
                    } 

                    return true;
                }
            }
        }

        public bool IsFree
        {
            get
            {
                if (this.IsIAP) return false;

                if (this.Wallet != null && !this.Wallet.IsEmpty) return false;

                if (this.AvatarPiece != null && this.AvatarPiece.Pieces > 0) return false;

                return true;
            }
        }

        /// <summary>
        /// Check if the user has could buy the store product.
        /// Always return true for IAP product. No Internet connection, etc are checked.
        /// </summary>
        /// <returns><c>true</c>, if the user may purchase the product, <c>false</c> otherwise.</returns>
        /// <param name="user">User.</param>
        public bool CanBePurchasedBy(UserLogin user)
        {
            return this.CanBePurchasedWith(user.Wallet, user.UserAvatars);
        }

		/// <summary>
		/// Check if the user has could buy the store product.
		/// Always return true for IAP product. No Internet connection, etc are checked.
		/// </summary>
		/// <returns><c>true</c>, if the user may purchase the product, <c>false</c> otherwise.</returns>
		/// <param name="userWallet">User wallet.</param>
		/// <param name="userAvatars">User avatars.</param>
        public bool CanBePurchasedWith(Wallet userWallet, List<UserAvatar> userAvatars)
        {
			if (this.IsIAP) return true;

			if (!this.IsValid) return false;

			if (this.Wallet != null && !this.Wallet.IsEmpty)
			{
                if (userWallet == null) return false;

				if (userWallet.Gems < this.Wallet.Gems || userWallet.Gold < this.Wallet.Gold || userWallet.HappyTokens < this.Wallet.HappyTokens) return false;
			}

			if (this.AvatarPiece != null && this.AvatarPiece.Pieces > 0)
			{
			    var userAvatar = userAvatars?.Find(i => i.AvatarType == this.AvatarPiece.AvatarType);
				if (userAvatar == null || userAvatar.Pieces < this.AvatarPiece.Pieces) return false;
			}

			return true;
        }

        public bool PurchaseWith(Wallet userWallet = null, List<UserAvatar> userAvatars = null)
        {
            // make sure there is enough to pay for
            if (this.CanBePurchasedWith(userWallet, userAvatars)) 
            {
                if (this.Wallet != null && !this.Wallet.IsEmpty)
                {
                    if (userWallet == null) return false;

                    userWallet.PayFor(this.Wallet);
                }

                if (this.AvatarPiece != null && this.AvatarPiece.Pieces > 0)
                {
                    var userAvatar = userAvatars?.Find(i => i.AvatarType == this.AvatarPiece.AvatarType);
                    if (userAvatar == null) return false;

                    userAvatar.Pieces -= this.AvatarPiece.Pieces;
                }

                return true;
            }

            return false;
        }

        public override string ToString() => (Wallet == null || Wallet.IsEmpty ? "" : Wallet.ToString()) + (AvatarPiece == null || AvatarPiece.Pieces <= 0 ? "" : AvatarPiece.AvatarType + ":" + AvatarPiece.Pieces);
    }
}
