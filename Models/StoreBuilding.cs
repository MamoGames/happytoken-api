namespace HappyTokenApi.Models
{
    public class StoreBuilding
    {
        public BuildingType BuildingType { get; set; }

        public int Gold { get; set; }

        public bool IsVisible { get; set; }

        public bool IsPromoted { get; set; }
    }
}
