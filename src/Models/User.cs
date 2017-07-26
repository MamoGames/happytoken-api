﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTokenApi.Models
{
    public class User : Resource
    {
        public string UserId { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string DeviceId { get; set; }

        public string SessionToken { get; set; }
    }
}
