namespace HappyTokenApi.Models
{
    public class StoreBuildingUpgrade : StoreProduct
    {
        public BuildingType BuildingType { get; set; }

        public int Level { get; set; }
    }
}
