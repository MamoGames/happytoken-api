using System;
using HappyTokenApi.Models;

namespace HappyTokenApi
{
    public class StoreO2O : StoreProduct
    {
        public string ProductCode { get; set; }

        public string VendorProductCode { get; set; }

        public int Inventory { get; set; }
    }
}
