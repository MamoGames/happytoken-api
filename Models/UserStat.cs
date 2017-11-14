using System;
namespace HappyTokenApi.Models
{
    public class UserStat
    {
        public string StatName { get; set; }

        // value is multipied by 1000 if it is a long
        public long StatValue { get; set; }
    }
}
