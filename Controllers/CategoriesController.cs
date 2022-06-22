using AutoMapper;
using BlackMarket_API.Data.Interfaces;
using BlackMarket_API.Data.Models;
using BlackMarket_API.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace BlackMarket_API.Controllers
{
    //[Authorize]
    public class CategoriesController : ApiController
    {
        readonly ICategoryRepository categoryRepository;
        readonly IMapper mapper;

        public CategoriesController(ICategoryRepository categoryRepository, IMapper mapper)
        {
            this.categoryRepository = categoryRepository;
            this.mapper = mapper;
        }


		public CategoriesViewModel Get()
		{
			return categoryRepository.GetCategories(mapper);
		}

		public IHttpActionResult ChangeCategoryPhoto()
		{
			var httpRequest = HttpContext.Current.Request;
			if (httpRequest.Files.Count < 1)
			{
				return BadRequest("No image was given");
			}

			if (string.IsNullOrEmpty(httpRequest.Form["CategoryId"]))
			{
				return BadRequest("CategoryId was not given");
			}

			int categoryId = Int32.Parse(httpRequest.Form["CategoryId"]);
			HttpPostedFile photo = httpRequest.Files[0];

			string photoName = photo.FileName;
			Stream photoStream = photo.InputStream;

			bool success = categoryRepository.ChangeCategoryPhoto(categoryId, Path.GetExtension(photoName), photoStream);
			if (!success)
				return BadRequest("Couldn't change the image (it may have been error while saving photo to Azure Storage or the product doesn't exist)");

			return Ok();
		}

		public void Post()
		{
            //categoryRepository.AddCategory();
        }
    }
}
