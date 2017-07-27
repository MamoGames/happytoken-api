using System.Collections.Generic;
using ArangoDB.Client;
using HappyTokenApi.Models;

namespace HappyTokenApi.Data.Config.Entities
{
    [CollectionProperty(CollectionName = "Buildings", Naming = NamingConvention.UnChanged)]
    public class DbBuildings
    {
        [DocumentProperty(Identifier = IdentifierType.Key)]
        public string Key { get; set; }

        public List<Building> Buildings { get; set; }
    }
}
