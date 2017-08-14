namespace HappyTokenApi.Models
{
    public class StoreAvatar
    {
        public AvatarType AvatarType { get; set; }

        public int Gold { get; set; }

        public int Gems { get; set; }

        public int HappyTokens { get; set; }

        public bool IsVisible { get; set; }

        public bool IsPromoted { get; set; }
    }
}
