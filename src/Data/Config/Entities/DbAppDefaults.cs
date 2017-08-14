using ArangoDB.Client;
using HappyTokenApi.Models;

namespace HappyTokenApi.Data.Config.Entities
{
    [CollectionProperty(CollectionName = "AppDefaults", Naming = NamingConvention.UnChanged)]
    public class DbAppDefaults : AppDefaults
    {
        [DocumentProperty(Identifier = IdentifierType.Key)]
        public string Key { get; set; }
    }
}
