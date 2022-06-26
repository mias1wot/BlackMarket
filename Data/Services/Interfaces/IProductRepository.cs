using AutoMapper;
using BlackMarket_API.Data.Models;
using BlackMarket_API.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackMarket_API.Data.Services.Interfaces
{
	public interface IProductRepository
	{
		HomeProductsViewModel GetProducts(long userId, int page, int pageSize, IMapper mapper);
		OpenedProductViewModel GetProduct(long userId, long id, IMapper mapper);
		HomeProductsViewModel GetByCategory(long userId, int categoryId, int page, int pageSize, IMapper mapper);
		HomeProductsViewModel GetProductsByName(long userId, string name, int categoryId, int page, int pageSize, IMapper mapper);
		string AddProduct(string Name, decimal price, Stream photo, string photoExtension, int categoryId, string description, string extraDescription);
		string ChangeProduct(long productId, string name, decimal? price, Stream newPhoto, string photoExtension, int? categoryId, string description, string extraDescription);
		bool ChangeProductPhoto(long productId, string photoExtension, Stream newPhoto);
		string DeleteProduct(long productId);


		void Test(IMapper mapper);
	}
}
