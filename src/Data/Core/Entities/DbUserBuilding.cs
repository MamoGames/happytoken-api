using HappyTokenApi.Models;
using System.ComponentModel.DataAnnotations;

namespace HappyTokenApi.Data.Core.Entities
{
    public class DbUserBuilding : UserBuilding
    {
        [Key]
        public string UsersBuildingId { get; set; }

        [Required]
        public string UserId { get; set; }
    }
}
