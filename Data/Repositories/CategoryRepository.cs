using AutoMapper;
using BlackMarket_API.Data.Interfaces;
using BlackMarket_API.Data.Models;
using BlackMarket_API.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace BlackMarket_API.Data.Repositories
{
	public class CategoryRepository: ICategoryRepository
	{
		private const string _containerName = "categories";


		public CategoriesViewModel GetCategories(IMapper mapper)
		{
			using (BlackMarket context = new BlackMarket())
			{
				var categories = context.Category.ToList();

				var categoriesViewModel = new List<CategoryViewModel>();
				categories.ForEach(category => categoriesViewModel.Add(mapper.Map<CategoryViewModel>(category)));

				var photos = AzureStorage.GetPhotosFromAzureStorage(_containerName, categories.Select(category => category.PhotoPath).ToList());
				var photoIterator = photos.GetEnumerator();

				categoriesViewModel.ForEach(categoryViewModel =>
				{
					photoIterator.MoveNext();
					categoryViewModel.Photo = photoIterator.Current;
				});

				return new CategoriesViewModel
				{
					Categories = categoriesViewModel
				};
			}
		}

		public bool ChangeCategoryPhoto(int categoryId, string photoExtension, Stream newPhoto)
		{
			using (BlackMarket context = new BlackMarket())
			{
				var category = context.Category.Find(categoryId);
				if (category == null)
					return false;

				//if PhotoPath was smth else than ProductId + extension
				string fileName = category.CategoryId.ToString() + photoExtension;
				if (category.PhotoPath != fileName)
				{
					category.PhotoPath = fileName;
					context.SaveChanges();
				}

				return AzureStorage.UploadPhotoToAzureStorage(_containerName, category.PhotoPath, newPhoto, true);
			}
		}

		public void AddCategory()
		{
			using (BlackMarket context = new BlackMarket())
			{
				//try
				//{
				//	Category c = new Category() { Name = "Food", Description = "" };
				//	context.Category.Add(c);
				//	context.SaveChanges();
				//}
				//catch (DbEntityValidationException e)
				//{
				//	foreach (var eve in e.EntityValidationErrors)
				//	{
				//		Debug.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
				//			eve.Entry.Entity.GetType().Name, eve.Entry.State);
				//		foreach (var ve in eve.ValidationErrors)
				//		{
				//			Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
				//				ve.PropertyName, ve.ErrorMessage);
				//		}
				//	}
				//	throw;
				//}
			}
		}
	}
}