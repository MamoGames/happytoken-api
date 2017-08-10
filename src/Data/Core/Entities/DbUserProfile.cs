using HappyTokenApi.Models;
using System.ComponentModel.DataAnnotations;

namespace HappyTokenApi.Data.Core.Entities
{
    public class DbUserProfile : Profile
    {
        [Key]
        public string UsersProfileId { get; set; }

        public string UserId { get; set; }
    }
}
