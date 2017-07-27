using System.Collections.Generic;
using ArangoDB.Client;
using HappyTokenApi.Models;

namespace HappyTokenApi.Data.Config.Entities
{
    [CollectionProperty(CollectionName = "Cakes", Naming = NamingConvention.UnChanged)]
    public class DbCakes
    {
        [DocumentProperty(Identifier = IdentifierType.Key)]
        public string Key { get; set; }

        public List<Cake> Cakes { get; set; }
    }
}
