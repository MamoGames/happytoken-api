using System;
using System.Collections.Generic;

namespace HappyTokenApi.Models
{
    public class QuestRewards
    {
        public ValueRange Gold { get; set; }

        public ValueRange Gems { get; set; }

        public ValueRange Xp { get; set; }

        public ValueRange HappyTokens { get; set; }

        public Dictionary<AvatarType, ValueRange> AvatarPieces { get; set; }
    }
}
