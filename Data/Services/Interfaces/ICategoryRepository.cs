using AutoMapper;
using BlackMarket_API.Data.Models;
using BlackMarket_API.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackMarket_API.Data.Services.Interfaces
{
	public interface ICategoryRepository
	{
		CategoriesViewModel GetCategories(IMapper mapper);
		string AddCategory(string name, string description, string photoExtension, Stream photo);
		bool ChangeCategoryPhoto(int categoryId, string photoExtension, Stream newPhoto);
		string ChangeCategory(int categoryId, string name, string description, string photoExtension, Stream newPhoto);
		string DeleteCategory(int categoryId);
	}
}
