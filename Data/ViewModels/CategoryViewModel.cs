using BlackMarket_API.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlackMarket_API.Data.ViewModels
{
	public class CategoriesViewModel
	{
		public IEnumerable<CategoryViewModel> Categories { get; set; }
	}

	public class CategoryViewModel
	{
		public int CategoryId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }

		//made from PhotoPath
		public byte[] Photo { get; set; }
	}
}