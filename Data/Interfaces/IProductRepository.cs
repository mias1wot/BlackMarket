using BlackMarket_API.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackMarket_API.Data.Interfaces
{
	public interface IProductRepository
	{
		IEnumerable<Product> GetProducts();
		Product GetProduct(int id);
		void AddProduct(string Name, decimal price, string photo, int categoryId, string description, string extraDescription);
	}
}
