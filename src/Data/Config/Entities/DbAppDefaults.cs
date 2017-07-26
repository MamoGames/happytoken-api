using ArangoDB.Client;

namespace HappyTokenApi.Data.Config.Entities
{
    [CollectionProperty(CollectionName = "AppDefaults", Naming = NamingConvention.UnChanged)]
    public class DbAppDefaults
    {
        [DocumentProperty(Identifier = IdentifierType.Key)]
        public string Key { get; set; }

        public int NameMaxChars { get; set; }

        public int FriendsMaxCount { get; set; }

        public int MessageMaxChars { get; set; }
    }
}
