using System.Collections.Generic;
using ArangoDB.Client;
using HappyTokenApi.Models;

namespace HappyTokenApi.Data.Config.Entities
{
    [CollectionProperty(CollectionName = "UserDefaults", Naming = NamingConvention.UnChanged)]
    public class DbUserDefaults
    {
        [DocumentProperty(Identifier = IdentifierType.Key)]
        public string Key { get; set; }

        public Wallet Wallet { get; set; }

        public HappinessIndex HappinessIndex { get; set; }

        public List<AvatarType> AvatarTypes { get; set; }

        public List<BuildingType> BuildingTypes { get; set; }
    }
}
