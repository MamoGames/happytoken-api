using HappyTokenApi.Models;
using System.ComponentModel.DataAnnotations;

namespace HappyTokenApi.Data.Core.Entities
{
    public class DbUserStorePurchaseRecord : UserStorePurchaseRecord
    {
        [Key]
        public string UsersStorePurchaseRecordId { get; set; }

        public string UserId { get; set; }
    }
}
