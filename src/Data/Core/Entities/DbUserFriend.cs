using HappyTokenApi.Models;
using System.ComponentModel.DataAnnotations;

namespace HappyTokenApi.Data.Core.Entities
{
    public class DbUserFriend : Friend
    {
        [Key]
        public string UsersFriendId { get; set; }
    }
}
