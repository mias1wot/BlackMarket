using AutoMapper;
using BlackMarket_API.Data.Models;
using BlackMarket_API.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackMarket_API.Data.Interfaces
{
	public interface ICategoryRepository
	{
		CategoriesViewModel GetCategories(IMapper mapper);
		bool ChangeCategoryPhoto(int categoryId, string photoExtension, Stream newPhoto);
		void AddCategory();
	}
}
