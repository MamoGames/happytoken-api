namespace HappyTokenApi.Models
{
    public class ResourceMine
    {
        public string Name { get; set; }

        public ResourceMineType ResourceMineType { get; set; }

        public int Days { get; set; }

        public int Price { get; set; }

        public int AmountPerDay { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
        
        public string DetailedDescription { get; set; }

        public string PrefabId { get; set; }
    }
}
