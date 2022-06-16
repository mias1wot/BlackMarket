﻿using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BlackMarket_API.Data.BindingModels;
using BlackMarket_API.Data.Interfaces;
using BlackMarket_API.Data.Models;
using BlackMarket_API.Data.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;


namespace BlackMarket_API.Controllers
{
	//[Authorize]
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


		public ProductsController(IProductRepository productRepository)
		{
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

		public IHttpActionResult ChangeProductPhoto()
		{
			//var form = HttpContext.Current.Request.Form;
			//var paramss = HttpContext.Current.Request.Params;

			var httpRequest = HttpContext.Current.Request;
			if (httpRequest.Files.Count < 1)
			{
				return BadRequest("No image was given");
			}

			if (string.IsNullOrEmpty(httpRequest.Form["ProductId"]))
			{
				return BadRequest("ProductId was not given");
			}

			int productId = Int32.Parse(httpRequest.Form["ProductId"]);
			HttpPostedFile photo = httpRequest.Files[0];

			string photoName = photo.FileName;
			Stream photoStream = photo.InputStream;

			bool success = productRepository.ChangeProductPhoto(productId, Path.GetExtension(photoName), photoStream);
			if (!success)
				return BadRequest("Couldn't change the image (it may have been error while saving photo to Azure Storage)");

			return Ok();
		}




		//test area: this is not implemented
		public void Test()
		{
			productRepository.Test();
		}

		//Sends image to Client. This works.
		public object GetImage()
		{
			var photoPath = "Product\\04b017b5fde4d2802cb5d405e4dd2860.png";
			var physicalPathToPhoto = HttpContext.Current.Server.MapPath("~\\wwwroot\\" + photoPath);

			var photo = System.IO.File.ReadAllBytes(physicalPathToPhoto);

			return new
			{
				BytePhoto = photo,
			};
		}
	}
}
