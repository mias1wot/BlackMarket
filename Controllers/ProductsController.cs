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

		


		public async Task<ProductsViewModel> Get()
		{
			User user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

			if(user == null)
			{
				return null;
			}

			return new ProductsViewModel
			{
				Products = productRepository.GetProducts()
			};
		}

		public async Task<Product> Get(int id)
		{
			return productRepository.GetProduct(id);
		}
	}
}
