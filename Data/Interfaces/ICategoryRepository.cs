using BlackMarket_API.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackMarket_API.Data.Interfaces
{
	public interface ICategoryRepository
	{
		IEnumerable<Category> GetCategories();
		void AddCategory();
	}
}
