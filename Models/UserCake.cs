using System;

namespace HappyTokenApi.Models
{
    public class UserCake
    {
        public CakeType CakeType { get; set; }

        public bool IsBaked { get; set; }

        public DateTime BakedDate { get; set; }
    }
}
