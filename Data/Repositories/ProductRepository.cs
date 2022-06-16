using AutoMapper;
using Azure.Storage.Blobs;
using BlackMarket_API.Data.Interfaces;
using BlackMarket_API.Data.Models;
using BlackMarket_API.Data.ViewModels;
using BlackMarket_API.ExtensionMethods;
using DevTrends.DataHelpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

//var physicalPathToPhoto = HttpContext.Current.Server.MapPath("~\\wwwroot\\" + res.Product.PhotoPath);
//var photo = File.ReadAllBytes(physicalPathToPhoto);


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

		private List<(Product Product, int SoldAmount, bool InCart)> GenericGetProductsFromDb(long userId, long? productId, int? categoryId, string productName, int page, int pageSize, IMapper mapper)
		{
			//no aggregation, no sum, just plain map object to object.
			//But this builds query that will return only needed fields for ViewModel from database
			//While automapper processes data after they're received from DB
			//You need to write complex tasks by your own
			//File: EnhancedAutomapper/QueryableExtensions.cs
			//var res = context.Product.Project().To<ProductViewModel>().ToList();



			using (BlackMarket context = new BlackMarket())
			{
				IQueryable<Product> products = context.Product;

				if (categoryId != null)
					products = products.Where(product => product.CategoryId == categoryId);

				if (productId != null)
					products = products.Where(product => product.ProductId == productId);

				if (productName != null)
					products = products.Where(product => product.Name.Contains(productName));


				var res = products
				.GroupJoin(context.Cart,
					product => product.ProductId,
					cart => cart.ProductId,
					(product, cartCollection) => new
					{
						Product = product,
						SoldAmount = cartCollection.Sum(cart => (int?)cart.Amount) ?? 0,
						InCart = cartCollection.Select(cart => cart.UserId).Contains(userId)
					})
				.OrderBy(productVM => productVM.Product.Name)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToList();


				return res.Select(productAndOther =>
					(
						Product: productAndOther.Product,
						SoldAmount: productAndOther.SoldAmount,
						InCart: productAndOther.InCart
					)).ToList();
			}
		}

		private ProductsViewModel GenericGetProductsViewModel(List<(Product Product, int SoldAmount, bool InCart)> products, IMapper mapper)
		{
			//Gets only needed fields to ViewModel
			var productsVM = new List<ProductViewModel>();

			products.ForEach(productAndOther =>
			{
				var productVM = mapper.Map<ProductViewModel>(productAndOther.Product);
				productVM.SoldAmount = productAndOther.SoldAmount;
				productVM.InCart = productAndOther.InCart;

				productsVM.Add(productVM);
			});


			//Gets products photo
			var photos = GetProductsPhotoFromAzureStorage(products.Select(productAndOther => productAndOther.Product.PhotoPath).ToList());
			var photoIterator = photos.GetEnumerator();

			productsVM.ForEach(productVM =>
			{
				photoIterator.MoveNext();
				productVM.Photo = photoIterator.Current;
			});


			//Gets products photo - previously used methods
			//res.ForEach(async productVM => productVM.Photo = await GetProductPhotoFromAzureStorage(productVM.Product.PhotoPath));
			//res.AsParallel().ForAll(productVM => productVM.Photo = GetProductPhotoFromAzureStorage(productVM.Product.PhotoPath));


			return new ProductsViewModel
			{
				Products = productsVM
			};
		}


		public ProductsViewModel GetProducts(long userId, int page, int pageSize, IMapper mapper)
		{
			//Gets data from DB
			List<(Product Product, int SoldAmount, bool InCart)> res =
				GenericGetProductsFromDb(
					userId: userId,
					productId: null,
					categoryId: null,
					productName: null,
					page, pageSize, mapper);

			//Gets only needed fields to ViewModel
			return GenericGetProductsViewModel(res, mapper);
		}

		public ProductViewModel GetProduct(long userId, long id, IMapper mapper)
		{
			//Gets data from DB
			List<(Product Product, int SoldAmount, bool InCart)> res =
				GenericGetProductsFromDb(
					userId: userId,
					productId: id,
					categoryId: null,
					productName: null,
					page: 1,
					pageSize: 1,
					mapper);


			//no product with this Id
			if (res.Count == 0)
				return null;

			//Gets only needed fields to ViewModel
			return GenericGetProductsViewModel(res, mapper).Products.FirstOrDefault();
		}

		public ProductsViewModel GetByCategory(long userId, int categoryId, int page, int pageSize, IMapper mapper)
		{
			//Gets data from DB
			List<(Product Product, int SoldAmount, bool InCart)> res =
				GenericGetProductsFromDb(
					userId: userId,
					productId: null,
					categoryId: categoryId,
					productName: null,
					page, pageSize, mapper);

			//Gets only needed fields to ViewModel
			return GenericGetProductsViewModel(res, mapper);
		}

		public ProductsViewModel GetProductsByName(long userId, string name, int categoryId, int page, int pageSize, IMapper mapper)
		{
			//Gets data from DB
			List<(Product Product, int SoldAmount, bool InCart)> res =
				GenericGetProductsFromDb(
					userId: userId,
					productId: null,
					categoryId: (categoryId != 0 ? (int?)categoryId : null),
					productName: name,
					page, pageSize, mapper);

			//Gets only needed fields to ViewModel
			return GenericGetProductsViewModel(res, mapper);
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
		List<byte[]> GetProductsPhotoFromAzureStorage(List<string> photoNameList)
		{
			var photos = new List<byte[]>();

			BlobServiceClient blobServiceClient = new BlobServiceClient(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
			BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("products");
			BlobClient blobClient;
			foreach (string photoName in photoNameList)
			{
				blobClient = containerClient.GetBlobClient(photoName);
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

					photos.Add(photo);
				}
				else
				{
					photos.Add(null);
				}
			}

			return photos;
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
