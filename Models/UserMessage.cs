using System;

namespace HappyTokenApi.Models
{
    public class UserMessage
    {
        public string UsersMessageId { get; set; }

        public string FromUserId { get; set; }

        public string ToUserId { get; set; }

        public MessageType MessageType { get; set; }

        public string Subject { get; set; }

        public string Preview { get; set; }

        public string Body { get; set; }

        public bool IsRead { get; set; }

        public DateTime ExpiryDate { get; set; }

        public string Image { get; set; }

        public string Shortcut { get; set; }

        public string LinkUrl { get; set; }

        public bool IsVideo { get; set; }

        public string VideoUrl { get; set; }

        public int HappyTokens { get; set; }

        public int Gold { get; set; }

        public int Gems { get; set; }

        public int Xp { get; set; }

        public AvatarType AvatarType { get; set; }

        public int AvatarPieces { get; set; }

        public CakeType PinkyCakeType { get; set; }

        public int PinkyCakes { get; set; }

        public bool IsDeleted { get; set; }
    }
}
