using ArangoDB.Client;

namespace HappyTokenApi.Data.Config.Entities
{
    [CollectionProperty(CollectionName = "UserDefaults", Naming = NamingConvention.UnChanged)]
    public class DbUserDefaults
    {
        [DocumentProperty(Identifier = IdentifierType.Key)]
        public string Key { get; set; }

        public int Xp { get; set; }

        public int Gold { get; set; }

        public int HappyTokens { get; set; }

        public int Gems { get; set; }

        public int Happiness { get; set; }
    }
}
