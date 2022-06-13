namespace BlackMarket_API.Migrations
{
	using BlackMarket_API.Data.Models;
	using Microsoft.AspNet.Identity;
	using Microsoft.AspNet.Identity.EntityFramework;
	using System;
	using System.Collections.Generic;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using System.Linq;

	internal sealed class Configuration : DbMigrationsConfiguration<BlackMarket_API.Data.Models.BlackMarket>
	{
		public Configuration()
		{
			AutomaticMigrationsEnabled = false;
		}

		protected override void Seed(BlackMarket_API.Data.Models.BlackMarket context)
		{
			//  This method will be called after migrating to the latest version.

			//  You can use the DbSet<T>.AddOrUpdate() helper extension method 
			//  to avoid creating duplicate seed data.


			//Add admin to DB
			string adminEmail = "user1@gmail.com";
			var userStore = new CustomUserStore(context);
			var userManager = new UserManager(userStore);
			User user = userManager.FindByEmail(adminEmail);
			if (user == null)
			{
				user = new User() { UserName = adminEmail, Email = adminEmail, Name = "Mykhailo", Surname = "Kachan" };
				userManager.Create(user, "Qwerty_1");
			}
			context.SaveChanges();


			//Add categories to DB
			List<Category> categories = new List<Category>
			{
				new Category() { Name = "Accessories", Description = "" },
				new Category() { Name = "Gifts", Description = "" },
				new Category() { Name = "Food", Description = "" },
				new Category() { Name = "Diet", Description = "" },
				new Category() { Name = "Beaty", Description = "" },
				new Category() { Name = "Clothes", Description = "" },
				new Category() { Name = "Gaming", Description = "" },
				new Category() { Name = "Coffee", Description = "" }
			};
			
			context.Category.AddOrUpdate(category => category.Name, categories.ToArray());
			context.SaveChanges();


			//Add products to DB
			List<Product> products = new List<Product>
			{
				//new Product() { Name = "", Price = M,  PhotoPath = "bulk", CategoryId = , Description = "" },
				new Product() { Name = "Wiley Saddle Bag - Fossil", Price = 180M, PhotoPath = "bulk", CategoryId = CategoryFrom(categories, "Accessories"), Description = "" },
				new Product() { Name = "UFO Basket Bag by MAM", Price = 180M,  PhotoPath = "bulk", CategoryId = CategoryFrom(categories, "Accessories"), Description = "" },
				new Product() { Name = "Recycled Nylon in Black", Price = 220M,  PhotoPath = "bulk", CategoryId = CategoryFrom(categories, "Accessories"), Description = "" },
				new Product() { Name = "Gift Box", Price = 0.99M,  PhotoPath = "bulk", CategoryId = CategoryFrom(categories, "Gifts"), Description = "" },
				new Product() { Name = "Zakarpatc'ki Mlynchiky", Price = 10M,  PhotoPath = "bulk", CategoryId = CategoryFrom(categories, "Food"), Description = "" },
				new Product() { Name = "Chicken Pie", Price = 15M,  PhotoPath = "bulk", CategoryId = CategoryFrom(categories, "Food"), Description = "" }
			};

			context.Product.AddOrUpdate(product => product.Name, products.ToArray());
			context.SaveChanges();


			//Add products to Cart in DB
			context.Cart.AddOrUpdate(cart => new { cart.ProductId, cart.UserId },
				//new Cart() { UserId = , ProductId = ProductFrom(products, ""), Amount = },
				new Cart() { UserId = user.Id, ProductId = ProductFrom(products, "Wiley Saddle Bag - Fossil"), Amount = 2},
				new Cart() { UserId = user.Id, ProductId = ProductFrom(products, "UFO Basket Bag by MAM"), Amount = 5},
				new Cart() { UserId = user.Id, ProductId = ProductFrom(products, "Gift Box"), Amount = 1}
			);
			context.SaveChanges();
		}

		private int CategoryFrom(List<Category> categories, string categoryName)
		{
			return categories.Where(category => category.Name == categoryName).Select(category => category.CategoryId).First();
		}
		private long ProductFrom(List<Product> products, string productName)
		{
			return products.Where(product => product.Name == productName).Select(product => product.ProductId).First();
		}
	}
}
