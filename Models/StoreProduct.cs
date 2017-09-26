namespace HappyTokenApi.Models
{
    public class StoreProduct
    {
        public string ProductId { get; set; }

        public string Name { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string DetailedDescription { get; set; }

        public string PrefabId { get; set; }

        public string Category { get; set; }

        public string Subcategory { get; set; }

        public bool IsVisible { get; set; }

        public bool IsO2O { get; set; }

		public bool IsHighlighted { get; set; }

		public StoreProductCost Cost { get; set; }

        public StoreProductRequirements Requirements { get; set; }

        /// <summary>
        /// Whether the product should be shown to user. Used by client side.
        /// </summary>
        /// <returns><c>true</c>, if the product should be shown for purchase, <c>false</c> otherwise.</returns>
        /// <param name="user">User.</param>
        public bool IsVisibleTo(UserLogin user)
        {
            return this.IsVisible && this.Requirements.IsMet(this.ProductId, user); 
        }
    }
}