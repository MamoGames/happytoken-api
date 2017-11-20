using System;
using System.Collections.Generic;

namespace HappyTokenApi.Models
{
    public class UserLogin
    {
        public string UserId { get; set; }

        public Profile Profile { get; set; }

        public Wallet Wallet { get; set; }

        public Happiness Happiness { get; set; }

        public DailyRewards DailyRewards { get; set; }
    }
}
