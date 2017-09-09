using ArangoDB.Client;

namespace HappyTokenApi.Data.Config.Entities
{
    [CollectionProperty(CollectionName = "Versions", Naming = NamingConvention.UnChanged)]
    public class DbVersions
    {
        [DocumentProperty(Identifier = IdentifierType.Key)]
        public string Key { get; set; }

        public string AppDefaults { get; set; }

        public string UserDefaults { get; set; }

        public string Avatars { get; set; }

        public string Buildings { get; set; }

        public string Cakes { get; set; }

        public string Games { get; set; }

        public string News { get; set; }

        public string Quests { get; set; }

        public string Store { get; set; }

        public string Promotions { get; set; }
    }
}
