using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTokenApi.Data.Entities
{
    public class CoreDbSettings
    {
        public string Server { get; set; }

        public string Database { get; set; }

        public string Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string ConnectionString => $"Server={Server};Port={Port};Database={Database};Username={Username};Password={Password};";
    }
}
