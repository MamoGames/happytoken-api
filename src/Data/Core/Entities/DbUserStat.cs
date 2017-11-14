using System.ComponentModel.DataAnnotations;

namespace HappyTokenApi.Data.Core.Entities
{
    public class DbUserStat : UserStat
    {
        [Key]
        public string UserStatId { get; set; }

        public string UserId { get; set; }
    }
}

