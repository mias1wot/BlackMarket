using AutoMapper;
using AutoMapper.QueryableExtensions;
using Azure.Storage.Blobs;
using BlackMarket_API.Data.Interfaces;
using BlackMarket_API.Data.Models;
using BlackMarket_API.Data.ViewModels;
using BlackMarket_API.EnhancedAutomapperNS;
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

//Gets products photo - previously used methods for getting photo from Azure Storage
//res.ForEach(async productVM => productVM.Photo = await GetProductPhotoFromAzureStorage(productVM.Product.PhotoPath));
//res.AsParallel().ForAll(productVM => productVM.Photo = GetProductPhotoFromAzureStorage(productVM.Product.PhotoPath));


namespace BlackMarket_API.Data.Repositories
{
	//Warning: here we have LINQ Skip method which takes *int* as a parameter. Your Product has long id. 
	//If product amount exceeds int.MaxValue, you need to implement and use BigSkip (just Skip in cycle).
	public class ProductRepository: IProductRepository
	{
		private const string _containerName = "products";


		//Delete this method "Test" when you're done
		public void Test(IMapper mapper)
		{
			return;

			//no aggregation, no sum, just plain map object to object.
			//But this builds query that will return only needed fields for ViewModel from database
			//While automapper processes data after they're received from DB
			//You need to write complex tasks by your own
			//File: EnhancedAutomapper/QueryableExtensions.cs
			//var res = context.Product.Project().To<ProductViewModel>().ToList();


			/*This shows how the code should look like for client to use */
			//Using EnhancedAutomapper
			//using (BlackMarket context = new BlackMarket())
			//{
			//	//var res = EnhancedAutomapper.Map<Product, ProductViewModel>(context.Product);
			//	//var query = EnhancedAutomapper.MapFrom(context.Product).To<ProductViewModel>();
			//	//var query = context.Product.EnhancedMap<Product, ProductViewModel>();
			//	var query = context.Product.EnhancedMap().To<ProductViewModel>();

			//	var res = query.ToList();
			//}

			using (BlackMarket context = new BlackMarket())
			{
				//var res = EnhancedAutomapper.Map<Product, ProductViewModel>(context.Product);
				//var query = EnhancedAutomapper.MapFrom(context.Product).To<ProductViewModel>();
				//var query = context.Product.EnhancedMap<Product, ProductViewModel>();

				//var query = context.Product.EnhancedMap().To<ProductViewModel>();


				//var e = context.Product.AsEnumerable().Select(product => mapper.Map<Product, ProductViewModel>(product));
				//var ee = context.Product.ProjectTo<ProductViewModel>();
				//var ter = ee.ToList();
				//var t = 43;

				var query = context.Product
					.GroupJoin(context.Cart,
					product => product.ProductId,
					cart => cart.ProductId,
					(product, cartCollection) => new
					{
						//productViewModel = EnhancedAutomapperFrom<Product>.TestTo2<ProductViewModel>(product),
						//productViewModel = EnhancedAutomapper.Test<Product, ProductViewModel>(product),
						//productViewModel = product.Test<Product, ProductViewModel>(),
						//productViewModel = ((IQueryable)new List<Product>() { product }).Test()
						//productViewModel = ((IQueryable<Product>)new List<Product>() { product }).Select(EnhancedAutomapper.Test<Product, ProductViewModel>()),
						//productViewModel = Queryable.Select(cartCollection, EnhancedAutomapper.Test<Product, ProductViewModel>()),
						//productViewModel = Enumerable.Select(product, EnhancedAutomapper.Test<Product, ProductViewModel>()),
						//productViewModel = Enumerable.Select<Product, ProductViewModel>(new List<Product>() { product }, EnhancedAutomapper.Test<Product, ProductViewModel>().Compile()),
						//productViewModel = EnhancedAutomapper.Test<Product, ProductViewModel>(),
						//productViewModel = Enumerable.Select<Product, ProductViewModel>(new List<Product>(), EnhancedAutomapper.Test<Product, ProductViewModel>(() => SoldAmount = cartCollection.Sum(cart => (int?)cart.Amount) ?? 0).Compile()),
						//productViewModel = Queryable.Select<Product, ProductViewModel>(null, EnhancedAutomapper.Test<Product, ProductViewModel>())),

						//Product = product,
						//ProductViewModel = new List<Product>() { product }.Select(product1 => EnhancedAutomapperFrom<Product>.TestTo<ProductViewModel>(product1)).First(),
						//Price = new List<Product>() { product }.Select(product1 => product1.Price),
						//Pr = Enumerable.Select(product, (Product product1) => product1.Price)
						//Product = product,
						SoldAmount = cartCollection.Sum(cart => (int?)cart.Amount) ?? 0,
						InCart = cartCollection.Select(cart => cart.UserId).Contains(0)
					});

				var res = query.ToList();
				//var a = "kfdj";
			}
		}

		private List<(Product Product, int SoldAmount, bool InCart)> GenericGetProductsFromDb(long userId, int? categoryId, string productName, int page, int pageSize, IMapper mapper)
		{
			using (BlackMarket context = new BlackMarket())
			{
				IQueryable<Product> products = context.Product;

				if (categoryId != null)
					products = products.Where(product => product.CategoryId == categoryId);

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

		private HomeProductsViewModel GenericGetHomeProductsViewModel(List<(Product Product, int SoldAmount, bool InCart)> products, IMapper mapper)
		{
			//Gets only needed fields to ViewModel
			var productViewModels = new List<HomeProductViewModel>();

			products.ForEach(productAndOther =>
			{
				var productVM = mapper.Map<HomeProductViewModel>(productAndOther.Product);
				productVM.SoldAmount = productAndOther.SoldAmount;
				productVM.InCart = productAndOther.InCart;

				productViewModels.Add(productVM);
			});


			//Gets products photo
			var photos = AzureStorage.GetPhotosFromAzureStorage(_containerName, products.Select(productAndOther => productAndOther.Product.PhotoPath).ToList());
			var photoIterator = photos.GetEnumerator();

			productViewModels.ForEach(productVM =>
			{
				photoIterator.MoveNext();
				productVM.Photo = photoIterator.Current;
			});



			return new HomeProductsViewModel
			{
				Products = productViewModels
			};
		}


		public HomeProductsViewModel GetProducts(long userId, int page, int pageSize, IMapper mapper)
		{
			//Gets data from DB
			List<(Product Product, int SoldAmount, bool InCart)> res =
				GenericGetProductsFromDb(
					userId: userId,
					categoryId: null,
					productName: null,
					page, pageSize, mapper);

			//Gets only needed fields to ViewModel
			return GenericGetHomeProductsViewModel(res, mapper);
		}

		public OpenedProductViewModel GetProduct(long userId, long id, IMapper mapper)
		{
			using(BlackMarket context = new BlackMarket())
			{
				//Gets data from DB
				var extendedProduct = context.Product
					.Where(product => product.ProductId == id)
					.GroupJoin(context.Cart,
						product => product.ProductId,
						cart => cart.ProductId,
						(product, cartCollection) => new
						{
							Product = product,
							SoldAmount = cartCollection.Sum(cart => (int?)cart.Amount) ?? 0,
							InCart = cartCollection.Select(cart => cart.UserId).Contains(userId)
						})
					.Join(context.Category,
						productAndOther => productAndOther.Product.CategoryId,
						category => category.CategoryId,
						(productAndOther, category) => new
						{
							Product = productAndOther.Product,
							SoldAmount = productAndOther.SoldAmount,
							InCart = productAndOther.InCart,
							CategoryName = category.Name
						})
					.FirstOrDefault();


				//no product with this Id
				if (extendedProduct == null)
					return null;


				//Gets ViewModel
				var openedProductVM = mapper.Map<OpenedProductViewModel>(extendedProduct.Product);
				openedProductVM.CategoryName = extendedProduct.CategoryName;
				openedProductVM.SoldAmount = extendedProduct.SoldAmount;
				openedProductVM.InCart = extendedProduct.InCart;


				//Gets product photo
				var photos = AzureStorage.GetPhotosFromAzureStorage(_containerName, new List<string>() { extendedProduct.Product.PhotoPath });
				openedProductVM.Photo = photos.FirstOrDefault();


				return openedProductVM;
			}
		}

		public HomeProductsViewModel GetByCategory(long userId, int categoryId, int page, int pageSize, IMapper mapper)
		{
			//Gets data from DB
			List<(Product Product, int SoldAmount, bool InCart)> res =
				GenericGetProductsFromDb(
					userId: userId,
					categoryId: categoryId,
					productName: null,
					page, pageSize, mapper);

			//Gets only needed fields to ViewModel
			return GenericGetHomeProductsViewModel(res, mapper);
		}

		public HomeProductsViewModel GetProductsByName(long userId, string name, int categoryId, int page, int pageSize, IMapper mapper)
		{
			//Gets data from DB
			List<(Product Product, int SoldAmount, bool InCart)> res =
				GenericGetProductsFromDb(
					userId: userId,
					categoryId: (categoryId != 0 ? (int?)categoryId : null),
					productName: name,
					page, pageSize, mapper);

			//Gets only needed fields to ViewModel
			return GenericGetHomeProductsViewModel(res, mapper);
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

				if (product == null)
					return false;

				//if PhotoPath was smth else than ProductId + extension
				string fileName = product.ProductId.ToString() + photoExtension;
				if(product.PhotoPath != fileName)
				{
					product.PhotoPath = fileName;
					context.SaveChanges();
				}
				
				return AzureStorage.UploadPhotoToAzureStorage(_containerName, product.PhotoPath, newPhoto, true);
			}
		}
	}
}
