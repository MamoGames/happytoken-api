using System.ComponentModel.DataAnnotations;
using HappyTokenApi.Models;

namespace HappyTokenApi.Data.Core.Entities
{
    public class DbUserStat : UserStat
    {
        [Key]
        public string UsersStatId { get; set; }

        public string UserId { get; set; }
    }
}

