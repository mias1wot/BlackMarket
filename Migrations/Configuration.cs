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
				new Category() { Name = "Accessories", PhotoPath = "1.jpg", Description = "" },
				new Category() { Name = "Gifts", PhotoPath = "2.png", Description = "" },
				new Category() { Name = "Food", PhotoPath = "3.png", Description = "" },
				new Category() { Name = "Diet", PhotoPath = "4.png", Description = "" },
				new Category() { Name = "Beaty", PhotoPath = "5.png", Description = "" },
				new Category() { Name = "Clothes", PhotoPath = "6.png", Description = "" },
				new Category() { Name = "Gaming", PhotoPath = "7.png", Description = "" },
				new Category() { Name = "Coffee", PhotoPath = "8.png", Description = "" },
			};
			
			context.Category.AddOrUpdate(category => category.Name, categories.ToArray());
			context.SaveChanges();


			//Add products to DB
			List<Product> products = new List<Product>
			{
				//new Product() { Name = "", Price = M,  PhotoPath = "", CategoryId = , Description = "" },
				new Product() { Name = "Wiley Saddle Bag - Fossil", Price = 180M, PhotoPath = "1.png", CategoryId = CategoryFrom(categories, "Accessories"), Description = "This will emphasize your royal style", ExtraDescription = "Oh my Goddble, German, are you crazy to miss this bargain??!" },
				new Product() { Name = "UFO Basket Bag by MAM", Price = 180M,  PhotoPath = "2.png", CategoryId = CategoryFrom(categories, "Accessories"), Description = "Rustic style is the style anyway. But this thing differes from rustic one by the price." , ExtraDescription = "Show them who is the king in YOUR village!" },
				new Product() { Name = "Recycled Nylon in Black", Price = 220M,  PhotoPath = "3.png", CategoryId = CategoryFrom(categories, "Accessories"), Description = "P - perfect" , ExtraDescription = "This is the perfect choice for travelling." },
				new Product() { Name = "Gift Box", Price = 0.99M,  PhotoPath = "4.jpg", CategoryId = CategoryFrom(categories, "Gifts"), Description = "Please your friends with beautiful present in gorgeous box" , ExtraDescription = "Invited to birthday party? Then that box is the must!" },
				new Product() { Name = "Zakarpatc'ki Mlynchiky", Price = 10M,  PhotoPath = "5.jpg", CategoryId = CategoryFrom(categories, "Food"), Description = "Like nothing in the world" , ExtraDescription = "Have you been to Transcarpathia? This is so delicious that I came here 10 years ago for a couple of days and couldn't leave because of these pancakes. Help me if you read this..." },
				new Product() { Name = "Chicken Pie", Price = 15M,  PhotoPath = "6.jpg", CategoryId = CategoryFrom(categories, "Food"), Description = "Do you like animals? Then you'll like this pie! We're all animals likers one way or another." , ExtraDescription = "Do you remember story about Zakarpatc'ki Mlynchiky? It's been 5 years since I wrote it. So many people came to help me. Most of them were defeated by pancakes, but the strongest ones were shocked and destroyed by this incredebly great pie. So there is no reason to decline buying it." },
				new Product() { Name = "Wiley Hair Hoop - Fossil", Price = 199.99M,  PhotoPath = "7.png", CategoryId = CategoryFrom(categories, "Accessories"), Description = "Hoop for girls (or boys if you like such things)" , ExtraDescription = "Don't know what to present your girlfriend on birthday? Take this one. Anyway you'll get socks for yours." },
				new Product() { Name = "Wiley Saddle Thing - Fossil", Price = 199.99M,  PhotoPath = "8.png", CategoryId = CategoryFrom(categories, "Accessories"), Description = "Ordinary black bag" , ExtraDescription = "But there are always skeletons in the cupboard of mysterious people buying this" },
				new Product() { Name = "Relly Christmar Tree Ball", Price = 20M,  PhotoPath = "9.png", CategoryId = CategoryFrom(categories, "Accessories"), Description = "Jingle bells, jingle bells, jingle all the way..." , ExtraDescription = "New year without Christmas tree and Christmas ball? You should be kidding me! Get this one, hand it on your tree and sing the songs! Even if it's summer. You have your own celebration schedule." },
				new Product() { Name = "Expensive Reggy Rag", Price = 20M,  PhotoPath = "10.png", CategoryId = CategoryFrom(categories, "Accessories"), Description = "Is it reference? Or programer guy decided to completely destroy Lui Viton? Who knows." , ExtraDescription = "You wannna be the junker king? Get this rag! It'll be the most popular rag in market soon." },
				new Product() { Name = "Wiley Hair Hoop - Fossil+", Price = 15M,  PhotoPath = "11.png", CategoryId = CategoryFrom(categories, "Accessories"), Description = "Do you see this gold glorious hoop? It can be yours. Think about it." , ExtraDescription = "Ahh, it reminds me of Egypt, 13 year of this era. Very wealthy girsl wore such hoops. Remember this as if it was yesterday. Cats, pharaons, vampires, alients drinking beer with me. Me having discussion with dogs about cats and their position in society. Brilliant time! Or that was yesterday when I got sunstroke? Ah, doesn't matter." },
				new Product() { Name = "Ricky Hair Hoop", Price = 15M,  PhotoPath = "12.png", CategoryId = CategoryFrom(categories, "Accessories"), Description = "This is a real thing. So flexible as a student when answers exam questions to a tutor. You see how this bends? It'll do anything just like Phaniliy Petrovych, gathering bottles near me now, for another bottle of vodka (ah, it's not him, that's just a student of NU LP. In this case it's nothing special)" , ExtraDescription = "Did you see Rick and Morty? The studio decided to make their own hoop. You really consider yourself a fan of this series if you didn't buy it? Come on, GO AND DO BUY! After you wear it, you're ready to show everyone who is the boss of the gym." },
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
