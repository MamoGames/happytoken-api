using System;

namespace HappyTokenApi.Models
{
    public class UserCake
    {
        public CakeType CakeType { get; set; }

        public bool IsBaked { get; set; }

        public DateTime BakedDate { get; set; }

        // The actual value (will be) resulted. Cakes could have different values even in the same type.
        public int Value { get; set; }
    }
}
