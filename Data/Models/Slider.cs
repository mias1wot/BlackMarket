using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BlackMarket_API.Data.Models
{
	[Table("Slider")]
	public partial class Slider
	{
		//Returned rows are always ordered by this SliderId
		[Key]
		[Column(Order = 0)]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int SliderNumber { get; set; }

		[Required]
		[StringLength(150)]
		public string PhotoPath { get; set; }
	}
}