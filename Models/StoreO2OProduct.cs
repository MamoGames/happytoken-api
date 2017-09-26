using System;
using HappyTokenApi.Models;

namespace HappyTokenApi.Models
{
    public class StoreO2OProduct : StoreProduct
    {
        public string ProductCode { get; set; }

        public string VendorProductCode { get; set; }

        public int Inventory { get; set; }
    }
}
