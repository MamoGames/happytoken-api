using HappyTokenApi.Models;
using System.ComponentModel.DataAnnotations;

namespace HappyTokenApi.Data.Core.Entities
{
    public class DbUserCakes : UserCake
    {
        [Key]
        public string UsersCakeId { get; set; }

        [Required]
        public string UserId { get; set; }
    }
}
