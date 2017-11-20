using System;

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

        public bool IsPromoted { get; set; }

        public StoreProductCost Cost { get; set; }

        public StoreProductRequirements Requirements { get; set; }

        public T Clone<T>() where T : StoreProduct
        {
            return (T)this.MemberwiseClone();
        }
    }
}