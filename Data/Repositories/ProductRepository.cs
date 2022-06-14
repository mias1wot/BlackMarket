using BlackMarket_API.Data.Interfaces;
using BlackMarket_API.Data.Models;
using BlackMarket_API.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BlackMarket_API.Data.Repositories
{
	//Warning: here we have Skip method which takes *int* as a parameter. Your Product has long id. 
	//If product amount exceeds int.MaxValue, you need to implement and use BigSkip (just Skip in cycle).
	public class ProductRepository: IProductRepository
	{
		public ProductsViewModel GetProducts(long userId, int page, int pageSize)
		{
			using (BlackMarket context = new BlackMarket())
			{
				//return context.Product.OrderBy(product => product.Name).ToList();

				var res = context.Product
					.GroupJoin(context.Cart,
						product => product.ProductId,
						cart => cart.ProductId,
						(product, cartCollection) => new ProductViewModel() {
							Product = product,
							SoldAmount = cartCollection.Sum(cart => (int?)cart.Amount) ?? 0,
							InCart = cartCollection.Select(cart => cart.UserId).Contains(userId)
						})
					.OrderBy(productVM => productVM.Product.Name)
					.Skip((page - 1) * pageSize)
					.Take(pageSize)
					.ToList();

				return new ProductsViewModel()
				{
					Products = res
				};
			}
		}
		
		public ProductViewModel GetProduct(long userId, long id)
		{
			using (BlackMarket context = new BlackMarket())
			{
				var res = context.Product
					.Where(product => product.ProductId == id)
					.GroupJoin(context.Cart,
						product => product.ProductId,
						cart => cart.ProductId,
						(product, cartCollection) => new ProductViewModel {
							Product = product,
							SoldAmount = cartCollection.Sum(cart => (int?)cart.Amount) ?? 0,
							InCart = cartCollection.Select(cart => cart.UserId).Contains(userId)
						})
					.FirstOrDefault();


				var physicalPathToPhoto = HttpContext.Current.Server.MapPath("~\\wwwroot\\" + res.Product.PhotoPath);

				var photo = File.ReadAllText(physicalPathToPhoto);
				//var photo2 = File.ReadAllLines(physicalPathToPhoto);
				var photo3 = File.ReadAllBytes(physicalPathToPhoto);

				return res;//can be null
			}
		}

		public ProductsViewModel GetByCategory(long userId, int categoryId, int page, int pageSize)
		{
			using (BlackMarket context = new BlackMarket())
			{
				var res = context.Product
					.Where(product => product.CategoryId == categoryId)
					.GroupJoin(context.Cart,
						product => product.ProductId,
						cart => cart.ProductId,
						(product, cartCollection) => new ProductViewModel() {
							Product = product,
							SoldAmount = cartCollection.Sum(cart => (int?)cart.Amount) ?? 0,
							InCart = cartCollection.Select(cart => cart.UserId).Contains(userId)
						})
					.OrderBy(productVM => productVM.Product.Name)
					.Skip((page - 1) * pageSize)
					.Take(pageSize)
					.ToList();

				return new ProductsViewModel()
				{
					Products = res
				};
			}
		}

		public ProductsViewModel GetProductsByName(long userId, string name, int categoryId, int page, int pageSize)
		{
			using (BlackMarket context = new BlackMarket())
			{
				IQueryable<Product> products = context.Product;

				if (categoryId != 0)
					products = products.Where(product => product.CategoryId == categoryId);


					var res = products
					.Where(product => product.Name.Contains(name))
					.GroupJoin(context.Cart,
						product => product.ProductId,
						cart => cart.ProductId,
						(product, cartCollection) => new ProductViewModel() {
							Product = product,
							SoldAmount = cartCollection.Sum(cart => (int?)cart.Amount) ?? 0,
							InCart = cartCollection.Select(cart => cart.UserId).Contains(userId)
						})
					.OrderBy(productVM => productVM.Product.Name)
					.Skip((page - 1) * pageSize)
					.Take(pageSize)
					.ToList();

				return new ProductsViewModel()
				{
					Products = res
				};
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
