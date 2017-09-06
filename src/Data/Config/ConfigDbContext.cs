using ArangoDB.Client;
using HappyTokenApi.Data.Config.Entities;
using HappyTokenApi.Models;
using Microsoft.Extensions.Options;
using System.Net;

namespace HappyTokenApi.Data.Config
{
    public class ConfigDbContext
    {
        public DbVersions Versions { get; private set; }

        public DbAppDefaults AppDefaults { get; private set; }

        public DbUserDefaults UserDefaults { get; private set; }

        public DbCakes Cakes { get; private set; }

        public DbAvatars Avatars { get; private set; }

        public DbBuildings Buildings { get; private set; }

        public DbStore Store { get; private set; }

        public ConfigDbContext(IOptions<ConfigDbSettings> options)
        {
            var configDbSettings = options.Value;

            ArangoDatabase.ChangeSetting(s =>
            {
                s.Database = configDbSettings.DbName;
                s.Url = configDbSettings.Url;
                s.Credential = new NetworkCredential(configDbSettings.UserName, configDbSettings.Password);
                s.SystemDatabaseCredential = new NetworkCredential(configDbSettings.UserName, configDbSettings.Password);
            });

            using (var db = ArangoDatabase.CreateWithSetting())
            {
                Versions = db.Document<DbVersions>(configDbSettings.BaseVersion);

                AppDefaults = db.Document<DbAppDefaults>(Versions.AppDefaults);

                UserDefaults = db.Document<DbUserDefaults>(Versions.UserDefaults);

                Cakes = db.Document<DbCakes>(Versions.Cakes);

                Avatars = db.Document<DbAvatars>(Versions.Avatars);

                Buildings = db.Document<DbBuildings>(Versions.Buildings);

                Store = db.Document<DbStore>(Versions.Store);
            }
        }
    }
}
