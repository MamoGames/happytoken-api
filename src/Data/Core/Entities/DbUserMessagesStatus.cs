using HappyTokenApi.Models;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Remove IDs that are no longer exist 
        /// </summary>
        /// <returns><c>true</c>, there are IDs removed, <c>false</c> otherwise.</returns>
        /// <param name="allMessageIds">All message identifiers.</param>
        public bool CleanUp(List<string> allMessageIds)
        {
            var newList = this.ReadMessageIds.ToList().Where(i => allMessageIds.Contains(i)).ToArray();

            if (newList.Length < this.ReadMessageIds.Length)
            {
                this.ReadMessageIds = newList;
                return true;
            }

            return false;
        }
    }
}
