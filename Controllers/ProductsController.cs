using BlackMarket_API.Data.Interfaces;
using BlackMarket_API.Data.Models;
using BlackMarket_API.Data.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace BlackMarket_API.Controllers
{
	[Authorize]
	public class ProductsController : ApiController
	{
		//readonly IProductRepository productRepository;

		//private UserManager _userManager;
		//public UserManager UserManager
		//{
		//	get
		//	{
		//		return _userManager ?? Request.GetOwinContext().GetUserManager<UserManager>();
		//	}
		//	private set
		//	{
		//		_userManager = value;
		//	}
		//}


		//public ProductsController(IProductRepository productRepository)
		//{
		//	this.productRepository = productRepository;
		//}

		//public ProductsController(UserManager userManager,
		//	IProductRepository productRepository)
		//{
		//	UserManager = userManager;
		//	this.productRepository = productRepository;
		//}
		private IProductRepository productRepository;

		private UserManager _userManager;
		public UserManager UserManager
		{
			get
			{
				return _userManager ?? Request.GetOwinContext().GetUserManager<UserManager>();
			}
			private set
			{
				_userManager = value;
			}
		}

		public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }


		public ProductsController(IProductRepository productRepository)
		{
			this.productRepository = productRepository;
		}

		public ProductsController(UserManager userManager,
			ISecureDataFormat<AuthenticationTicket> accessTokenFormat,
			IProductRepository productRepository)
		{
			UserManager = userManager;
			AccessTokenFormat = accessTokenFormat;
			this.productRepository = productRepository;
		}






		//page begins from 1
		public ProductsViewModel Get(int page, int pageSize)
		{
			return productRepository.GetProducts(User.Identity.GetUserId<long>(), page, pageSize);
		}

		//Returns null if there is no such a product
		public ProductViewModel Get(long id)
		{
			return productRepository.GetProduct(User.Identity.GetUserId<long>(), id);
		}

		public ProductsViewModel Get(int categoryId, int page, int pageSize)
		{
			return productRepository.GetByCategory(User.Identity.GetUserId<long>(), categoryId, page, pageSize);
		}

		public ProductsViewModel Get(string name, int page, int pageSize)
		{
			return Get(name, 0, page, pageSize);
		}
		public ProductsViewModel Get(string name, int categoryId, int page, int pageSize)
		{
			return productRepository.GetProductsByName(User.Identity.GetUserId<long>(), name, categoryId, page, pageSize);
		}
	}
}
