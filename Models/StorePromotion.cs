using System;
using System.Collections.Generic;

namespace HappyTokenApi.Models
{
    public class StorePromotion
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string DetailedDescription { get; set; }

        public string Image { get; set; }

        public string PrefabId { get; set; }

        public bool IsHighlighted { get; set; }

        public int Price { get; set; }

        public int Gold { get; set; }

        public int Gems { get; set; }

        public int HappyTokens { get; set; }

        public List<AvatarPiece> AvatarPieces { get; set; }
    }
}
