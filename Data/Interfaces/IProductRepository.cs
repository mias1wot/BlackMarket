using AutoMapper;
using BlackMarket_API.Data.Models;
using BlackMarket_API.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackMarket_API.Data.Interfaces
{
	public interface IProductRepository
	{
		ProductsViewModel GetProducts(long userId, int page, int pageSize, IMapper mapper);
		ProductViewModel GetProduct(long userId, long id, IMapper mapper);
		ProductsViewModel GetByCategory(long userId, int categoryId, int page, int pageSize, IMapper mapper);
		ProductsViewModel GetProductsByName(long userId, string name, int categoryId, int page, int pageSize, IMapper mapper);
		void AddProduct(string Name, decimal price, string photo, int categoryId, string description, string extraDescription);
		bool ChangeProductPhoto(int productId, string photoExtension, Stream newPhoto);
		void Test();
	}
}
