using BlackMarket_API.Data.Interfaces;
using BlackMarket_API.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace BlackMarket_API.Data.Repositories
{
	public class CategoryRepository: ICategoryRepository
	{
		public IEnumerable<Category> GetCategories()
		{
			using (BlackMarket context = new BlackMarket())
			{
				return context.Category.ToList();
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