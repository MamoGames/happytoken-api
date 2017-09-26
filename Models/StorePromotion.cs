using System;
using System.Collections.Generic;

namespace HappyTokenApi.Models
{
	public class StorePromotion
    {
        public StoreProductType StoreProductType { get; set; }

        public string PromotedProductID { get; set; }

		public string Name { get; set; }

		public string Title { get; set; }

		public string Description { get; set; }

		public string DetailedDescription { get; set; }

		public string PrefabId { get; set; }

        public string Code { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

		/// <summary>
		/// The discounted cost, use PromotedProductID to reference the original product price
		/// </summary>
		/// <value>The cost.</value>
		public StoreProductCost Cost { get; set; }
    }
}
