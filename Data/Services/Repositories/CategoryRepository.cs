using AutoMapper;
using BlackMarket_API.Data.Models;
using BlackMarket_API.Data.Services.Interfaces;
using BlackMarket_API.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace BlackMarket_API.Data.Services.Repositories
{
	public class CategoryRepository : ICategoryRepository
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


		//returns either error message or null
		public string AddCategory(string name, string description, string photoExtension, Stream photo)
		{
			using (BlackMarket context = new BlackMarket())
			using (var transaction = context.Database.BeginTransaction())
			{
				Category category = new Category
				{
					Name = name,
					Description = description,
				};
				context.Category.Add(category);
				context.SaveChanges();

				string photoName = category.CategoryId + photoExtension;

				bool azureUploadRes = AzureStorage.UploadPhotoToAzureStorage(_containerName, photoName, photo, false);
				if (!azureUploadRes)
					return "Failed to upload new photo to Azure Storage";

				category.PhotoPath = photoName;
				context.SaveChanges();

				transaction.Commit();

				return null;
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

		//returns either error message or null
		public string ChangeCategory(int categoryId, string name, string description, string photoExtension, Stream newPhoto)
		{
			using (BlackMarket context = new BlackMarket())
			{
				Category category = context.Category.Find(categoryId);
				if(category == null)
					return $"No Category with CategoryId = {categoryId} was found";

				if (!string.IsNullOrWhiteSpace(name))
					category.Name = name;
				if (!string.IsNullOrWhiteSpace(description))
					category.Description = description;

				if (newPhoto != null && !string.IsNullOrWhiteSpace(photoExtension))
				{
					string newPhotoName = category.PhotoPath + photoExtension;
					AzureStorage.DeletePhotoInAzureStorage(_containerName, category.PhotoPath);
					bool azureUploadRes = AzureStorage.UploadPhotoToAzureStorage(_containerName, newPhotoName, newPhoto, false);
					if (!azureUploadRes)
						return "Failed to upload new photo to Azure Storage";

					category.PhotoPath = newPhotoName;
				}

				context.SaveChanges();

				return null;
			}
		}

		//returns either error message or null
		public string DeleteCategory(int categoryId)
		{
			using (BlackMarket context = new BlackMarket())
			{
				Category category = context.Category.Find(categoryId);
				if (category == null)
					return $"No category with CategoryId = {categoryId} was found";

				context.Category.Remove(category);
				context.SaveChanges();
				return null;
			}
		}
	}
}