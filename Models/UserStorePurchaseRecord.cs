using System;
namespace HappyTokenApi.Models
{
    public class UserStorePurchaseRecord
    {
		public string StoreProductId { get; set; }

        public int Count { get; set; }

        public DateTime LastPurchase { get; set; }
    }
}
