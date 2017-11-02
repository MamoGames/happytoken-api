using HappyTokenApi.Models;
using System.ComponentModel.DataAnnotations;

namespace HappyTokenApi.Data.Core.Entities
{
    public class DbUserMessagesStatus 
    {
        [Key]
        public string UsersMessageStatusId { get; set; }

        [Required]
        public string UserId { get; set; } 

        public string[] ReadMessageIds { get; set; }

        public DbUserMessagesStatus()
        {
            this.ReadMessageIds = new string[0];
        }
    }
}
