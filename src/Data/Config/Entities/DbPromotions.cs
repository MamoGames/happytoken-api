using System.Collections.Generic;
using ArangoDB.Client;
using HappyTokenApi.Models;

namespace HappyTokenApi.Data.Config.Entities
{
    [CollectionProperty(CollectionName = "Promotions", Naming = NamingConvention.UnChanged)]
    public class DbPromotions
    {
        [DocumentProperty(Identifier = IdentifierType.Key)]
        public string Key { get; set; }

        public List<Promotion> Promotions { get; set; }
    }
}
