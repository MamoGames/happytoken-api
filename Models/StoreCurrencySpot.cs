namespace HappyTokenApi.Models
{
    public class StoreCurrencySpot
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

        public bool IsPromoted { get; set; }
    }
}
