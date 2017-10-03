using ArangoDB.Client;
using HappyTokenApi.Data.Config.Entities;
using HappyTokenApi.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Threading.Tasks;

namespace HappyTokenApi.Data.Config
{
    public class ConfigDbContext
    {
        private readonly ConfigDbSettings m_ConfigDbSettings;

        public DbVersions Versions { get; private set; }

        public DbAppDefaults AppDefaults { get; private set; }

        public DbUserDefaults UserDefaults { get; private set; }

        public DbCakes Cakes { get; private set; }

        public DbAvatars Avatars { get; private set; }

        public DbBuildings Buildings { get; private set; }

        public DbStore Store { get; private set; }

        public ConfigDbContext(IOptions<ConfigDbSettings> options)
        {
            m_ConfigDbSettings = options.Value;

            ArangoDatabase.ChangeSetting(s =>
            {
                s.Database = m_ConfigDbSettings.DbName;
                s.Url = m_ConfigDbSettings.Url;
                s.Credential = new NetworkCredential(m_ConfigDbSettings.UserName, m_ConfigDbSettings.Password);
                s.SystemDatabaseCredential = new NetworkCredential(m_ConfigDbSettings.UserName, m_ConfigDbSettings.Password);
            });

            RefreshConfig();
        }

        public void RefreshConfig()
        {
            using (var db = ArangoDatabase.CreateWithSetting())
            {
                Versions = db.Document<DbVersions>(m_ConfigDbSettings.BaseVersion);

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
