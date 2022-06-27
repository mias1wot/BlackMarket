using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BlackMarket_API.Data.Models
{
	[Table("Client")]
	public class Client
	{
        [Key]
        [MaxLength(30)]
        public string ClientId { get; set; }

        public string Secret { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public ApplicationTypes ApplicationType { get; set; }

        [Required]
        public bool Active { get; set; }

        [Required]
        public int RefreshTokenLifeTimeInMinutes { get; set; }

        [Required]
        [MaxLength(100)]
        public string AllowedOrigin { get; set; }
    }

    public enum ApplicationTypes
	{
        React = 0,
        Flutter = 1
	}
}