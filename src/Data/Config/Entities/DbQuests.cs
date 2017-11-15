using System.Collections.Generic;
using ArangoDB.Client;
using HappyTokenApi.Models;

namespace HappyTokenApi.Data.Config.Entities
{
    [CollectionProperty(CollectionName = "Quests", Naming = NamingConvention.UnChanged)]
    public class DbQuests
    {
        [DocumentProperty(Identifier = IdentifierType.Key)]
        public string Key { get; set; }

        public List<DbQuest> Quests { get; set; }
    }
}
