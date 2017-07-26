using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTokenApi.Data.Entities
{
    public class ConfigDbSettings
    {
        public string DbName { get; set; }

        public string Url { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string BaseVersion { get; set; }
    }
}
