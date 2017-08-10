using HappyTokenApi.Models;
using System.ComponentModel.DataAnnotations;

namespace HappyTokenApi.Data.Core.Entities
{
    public class DbUserWallet : Wallet
    {
        [Key]
        public string UsersWalletId { get; set; }

        public string UserId { get; set; }
    }
}
