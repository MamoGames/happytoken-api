using ArangoDB.Client;
using HappyTokenApi.Models;

namespace HappyTokenApi.Data.Config.Entities
{
    [CollectionProperty(CollectionName = "Store", Naming = NamingConvention.UnChanged)]
    public class DbStore : Store
    {
        [DocumentProperty(Identifier = IdentifierType.Key)]
        public string Key { get; set; }
    }
}
