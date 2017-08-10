using HappyTokenApi.Models;
using System.ComponentModel.DataAnnotations;

namespace HappyTokenApi.Data.Core.Entities
{
    public class DbUserHappiness : Happiness
    {
        [Key]
        public string UsersHappinessId { get; set; }

        public string UserId { get; set; }
    }
}
