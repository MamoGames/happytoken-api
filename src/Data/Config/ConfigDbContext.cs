using ArangoDB.Client;
using HappyTokenApi.Data.Config.Entities;
using HappyTokenApi.Models;
using System.Net;

namespace HappyTokenApi.Data.Config
{
    public class ConfigDbContext
    {
        private ConfigDbSettings m_ConfigDbSettings;

        public DbVersions Versions { get; private set; }

        public DbAppDefaults AppDefaults { get; private set; }

        public DbUserDefaults UserDefaults { get; private set; }

        public DbCakes Cakes { get; private set; }

        public DbAvatars Avatars { get; private set; }

        public DbBuildings Buildings { get; private set; }

        public DbStore Store { get; private set; }

        /// <summary>
        /// Specifies the version number used to derive all other config data versions.
        /// </summary>
        public ConfigDbContext(ConfigDbSettings options)
        {
            SetConfigDbSettings(options);

        }

        /// <summary>
        /// Sets the connection data for the config DB, such as the connection url and network credentials.
        /// </summary>
        public ConfigDbContext SetConfigDbSettings(ConfigDbSettings configDbSettings)
        {
            m_ConfigDbSettings = configDbSettings;

            return this;
        }

        /// <summary>
        /// Configures the ArangoDB database connection using the specified settings
        /// </summary>
        public ConfigDbContext ConfigureConnection()
        {
            ArangoDatabase.ChangeSetting(s =>
            {
                s.Database = m_ConfigDbSettings.DbName;
                s.Url = m_ConfigDbSettings.Url;
                s.Credential = new NetworkCredential(m_ConfigDbSettings.UserName, m_ConfigDbSettings.Password);
                s.SystemDatabaseCredential = new NetworkCredential(m_ConfigDbSettings.UserName, m_ConfigDbSettings.Password);
            });

            return this;
        }

        /// <summary>
        /// Loads all the config data from the config database into objects 
        /// </summary>
        public ConfigDbContext LoadConfigDataFromDb()
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

            return this;
        }
    }
}
