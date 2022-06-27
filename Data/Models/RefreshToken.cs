using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BlackMarket_API.Data.Models
{
	[Table("RefreshToken")]
	public class RefreshToken
	{
		[Key]
		public string RefreshTokenId { get; set; }

		[Required]
		public long UserId { get; set; }

		[Required]
		[MaxLength(30)]
		public string ClientId { get; set; }

		[Required]
		public string Ticket { get; set; }
	}
}