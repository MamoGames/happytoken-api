using System;

namespace HappyTokenApi.Models
{
    public class UserSendCakeMessage
    {
        public string ToUserId { get; set; }

        public CakeType CakeType { get; set; }
    }
}
