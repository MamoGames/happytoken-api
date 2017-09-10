using System;
using System.Collections.Generic;

namespace HappyTokenApi.Models
{
    public class AppConfig
    {
        public DateTime ServerDateTime { get; set; }

        public AppDefaults AppDefaults { get; set; }

        public List<Avatar> Avatars { get; set; }

        public List<Building> Buildings { get; set; }

        public List<Cake> Cakes { get; set; }

        public Store Store { get; set; }
    }
}
