using System.Collections.Generic;
using ArangoDB.Client;
using HappyTokenApi.Models;

namespace HappyTokenApi.Data.Config.Entities
{
    [CollectionProperty(CollectionName = "Avatars", Naming = NamingConvention.UnChanged)]
    public class DbAvatars
    {
        [DocumentProperty(Identifier = IdentifierType.Key)]
        public string Key { get; set; }

        public List<Avatar> Avatars { get; set; }
    }
}
