using System.ComponentModel.DataAnnotations;

namespace HappyTokenApi.Data.Core.Entities
{
    public class DbUser
    {
        [Key]
        public string UserId { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string DeviceId { get; set; }

        public string AuthToken { get; set; }
    }
}
