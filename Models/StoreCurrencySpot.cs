using System.Collections.Generic;

namespace HappyTokenApi.Models
{
    public class StoreCurrencySpot : StoreProduct
    {
        public Wallet Wallet { get; set; }

        public List<AvatarPiece> AvatarPieces { get; set; }
    }
}
