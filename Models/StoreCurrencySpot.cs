namespace HappyTokenApi.Models
{
    public class StoreCurrencySpot : IStoreProduct
    {
        /// <summary>
        /// Users buys this currency
        /// </summary>
        public CurrencyType BuyCurrencyType { get; set; }

        public int BuyAmount { get; set; }

        /// <summary>
        /// Store sells for this currency
        /// </summary>
        public CurrencyType SellCurrencyType { get; set; }

        public int SellAmount{ get; set; }

        public bool IsVisible { get; set; }

        public string Name { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string DetailedDescription { get; set; }

        public string PrefabId { get; set; }
    }
}
