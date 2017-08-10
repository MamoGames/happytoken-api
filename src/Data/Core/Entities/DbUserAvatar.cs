using HappyTokenApi.Models;
using System.ComponentModel.DataAnnotations;

namespace HappyTokenApi.Data.Core.Entities
{
    public class DbUserAvatar : UserAvatar
    {
        [Key]
        public string UsersAvatarId { get; set; }

        [Required]
        public string UserId { get; set; }
    }
}
