namespace HappyTokenApi.Models
{
    public class StoreAvatarUpgrade
    {
        public AvatarType AvatarType { get; set; }

        public int Level { get; set; }

        public int Gold { get; set; }

        public bool IsVisible { get; set; }
    }
}
