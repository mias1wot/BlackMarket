using BlackMarket_API.Data.Interfaces;
using BlackMarket_API.Data.Models;
using BlackMarket_API.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BlackMarket_API.Controllers
{
    [Authorize]
    public class CategoriesController : ApiController
    {
        readonly ICategoryRepository categoryRepository;
        public CategoriesController(ICategoryRepository categoryRepository)
        {
            this.categoryRepository = categoryRepository;
        }


        public CategoryViewModel Get()
		{
			return new CategoryViewModel()
            {
				Categories = categoryRepository.GetCategories()
        };
                
		}

        public void Post()
		{
            //categoryRepository.AddCategory();
        }
    }
}
