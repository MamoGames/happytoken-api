using System;
using System.Collections.Generic;

namespace HappyTokenApi.Models
{
    public class UserAvatar
    {
        public AvatarType AvatarType { get; set; }

        public int Pieces { get; set; }

        public int Level { get; set; }
    }
}
