using HappyTokenApi.Models;
using System.ComponentModel.DataAnnotations;

namespace HappyTokenApi.Data.Core.Entities
{
	public class DBUserDailyActions : UserDailyActions
	{
		[Key]
		public string UsersDailyActionId { get; set; }

		[Required]
		public string UserId { get; set; }
	}
}
