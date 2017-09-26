namespace HappyTokenApi.Models
{
    public class StoreAvatarUpgrade : StoreProduct
    {
        public AvatarType AvatarType { get; set; }

        public int Level { get; set; }
    }
}
