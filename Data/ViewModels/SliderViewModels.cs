using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlackMarket_API.Data.ViewModels
{
	public class SliderViewModel
	{
		public IEnumerable<SliderPhoto> Photos { get; set; }
	}
	public class SliderPhoto
	{
		public byte[] Photo { get; set; }
	}
}