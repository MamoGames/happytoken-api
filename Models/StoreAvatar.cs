namespace HappyTokenApi.Models
{
    public class StoreAvatar : IStoreProduct
    {
        public AvatarType AvatarType { get; set; }

        public int Gold { get; set; }

        public int Gems { get; set; }

        public int HappyTokens { get; set; }

        public bool IsVisible { get; set; }

        public string Name { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string DetailedDescription { get; set; }

        public string PrefabId { get; set; }
    }
}
