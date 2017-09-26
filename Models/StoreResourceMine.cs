namespace HappyTokenApi.Models
{
    public class ResourceMine : StoreProduct
    {
        public ResourceMineType ResourceMineType { get; set; }

        public int Days { get; set; }

        public int AmountPerDay { get; set; }

        public int AmountNow { get; set; }
    }
}
