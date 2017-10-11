using HappyTokenApi.Models;
using System.ComponentModel.DataAnnotations;

namespace HappyTokenApi.Data.Core.Entities
{
    public class DbUserMessage : UserMessage
    {
        [Key]
        public new string UsersMessageId { get; set; }
    }
}
