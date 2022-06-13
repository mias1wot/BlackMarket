using BlackMarket_API.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlackMarket_API.Data.ViewModels
{
	public class CategoryViewModel
	{
		public IEnumerable<Category> Categories { get; set; }
	}
}