using Azure.Storage.Blobs;
using BlackMarket_API.Data.Interfaces;
using BlackMarket_API.Data.Models;
using BlackMarket_API.Data.ViewModels;
using BlackMarket_API.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BlackMarket_API.Data.Repositories
{
	//Warning: here we have LINQ Skip method which takes *int* as a parameter. Your Product has long id. 
	//If product amount exceeds int.MaxValue, you need to implement and use BigSkip (just Skip in cycle).
	public class ProductRepository: IProductRepository
	{
		//Delete this method "Test" when you're done
		public void Test()
		{
			var file = "Product\\04b017b5fde4d2802cb5d405e4dd2860.png";
			var physicalPathToPhoto = HttpContext.Current.Server.MapPath("~\\wwwroot\\" + file);
			//var photo = File.ReadAllBytes(physicalPathToPhoto);
			var photoStream = File.OpenRead(physicalPathToPhoto);

			ChangeProductPhotoInAzureStorage("1", photoStream);
		}


		public ProductsViewModel GetProducts(long userId, int page, int pageSize)
		{
			using (BlackMarket context = new BlackMarket())
			{
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

				//Gets products photo
				//res.ForEach(async productVM => productVM.Photo = await GetProductPhotoFromAzureStorage(productVM.Product.PhotoPath));
				res.AsParallel().ForAll(productVM => productVM.Photo = GetProductPhotoFromAzureStorage(productVM.Product.PhotoPath));


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
						(product, cartCollection) => new ProductViewModel
						{
							Product = product,
							SoldAmount = cartCollection.Sum(cart => (int?)cart.Amount) ?? 0,
							InCart = cartCollection.Select(cart => cart.UserId).Contains(userId)
						})
					.FirstOrDefault();

				//no product with this Id
				if (res == null)
					return null;


				//var physicalPathToPhoto = HttpContext.Current.Server.MapPath("~\\wwwroot\\" + res.Product.PhotoPath);
				//var photo = File.ReadAllBytes(physicalPathToPhoto);
				
				res.Photo = GetProductPhotoFromAzureStorage(res.Product.PhotoPath);

				return res;
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

				//Gets products photo
				res.AsParallel().ForAll(productVM => productVM.Photo = GetProductPhotoFromAzureStorage(productVM.Product.PhotoPath));

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

				//Gets products photo
				res.AsParallel().ForAll(productVM => productVM.Photo = GetProductPhotoFromAzureStorage(productVM.Product.PhotoPath));

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

		public bool ChangeProductPhoto(int productId, string photoExtension, Stream newPhoto)
		{
			using(BlackMarket context = new BlackMarket())
			{
				var product = context.Product.Find(productId);

				//if PhotoPath was smth else than ProductId + extension
				string fileName = product.ProductId.ToString() + photoExtension;
				if(product.PhotoPath != fileName)
				{
					product.PhotoPath = fileName;
					context.SaveChanges();
				}
				
				return ChangeProductPhotoInAzureStorage(product.PhotoPath, newPhoto);
			}
		}



		//Gets product photo from Azure Blob Storage
		byte[] GetProductPhotoFromAzureStorage(string photoName)
		{
			BlobServiceClient blobServiceClient = new BlobServiceClient(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
			BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("products");
			BlobClient blobClient = containerClient.GetBlobClient(photoName);
			if (blobClient.Exists())
			{
				Stream fileStream = blobClient.Download().Value.Content;

				//Reads Stream as byte[]
				byte[] photo;
				using (var memoryStream = new MemoryStream())
				{
					fileStream.CopyTo(memoryStream);
					photo = memoryStream.ToArray();
				}

				return photo;
			}

			return null;
		}

		bool ChangeProductPhotoInAzureStorage(string photoName, Stream newPhoto)
		{
			BlobServiceClient blobServiceClient = new BlobServiceClient(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
			BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("products");
			BlobClient blobClient = containerClient.GetBlobClient(photoName);

			try
			{
				blobClient.Upload(newPhoto, true);
			}
			catch (Exception e)
			{
				return false;
			}

			return true;
		}
	}
}
