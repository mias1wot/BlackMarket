using BlackMarket_API.Data.Interfaces;
using BlackMarket_API.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackMarket_API.Data.Repositories
{
	public class ProductRepository: IProductRepository
	{
		public IEnumerable<Product> GetProducts()
		{
			using (BlackMarket context = new BlackMarket())
			{
				return context.Product.ToList();
			}
		}
		
		public Product GetProduct(int id)
		{
			using (BlackMarket context = new BlackMarket())
			{
				return context.Product.Where(product => product.ProductId == id).FirstOrDefault();
			}
		}

		public void AddProduct(string name, decimal price, string photo, int categoryId, string description, string extraDescription)
		{
			//using (BlackMarket context = new BlackMarket())
			//{
			//	Product product = new Product() 
			//	{ 
			//		Name = name, 
			//		Price = price, 
			//		PhotoPath = jkfdjf, 
			//		CategoryId = categoryId, 
			//		Description = description, 
			//		ExtraDescription = extraDescription
			//	};

			//	context.Product.Add(product);
			//	context.SaveChangesAsync();
			//}
		}
	}
}
