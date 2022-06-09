using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

//Do it before adding database migration to clear auto-created tables. They'll be recreated.
//drop table AspNetUserRoles
//drop table AspNetUserLogins
//drop table AspNetUserClaims
//drop table AspNetUsers
//drop table AspNetRoles



namespace BlackMarket_API.Data.Models
{
	public partial class BlackMarket : IdentityDbContext<User>
	{
		public BlackMarket()
			: base("name=BlackMarket")
		{
		}

		public static BlackMarket Create()
		{
			return new BlackMarket();
		}

		public virtual DbSet<Cart> Cart { get; set; }
		public virtual DbSet<Category> Category { get; set; }
		public virtual DbSet<Product> Product { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Category>()
				.HasMany(e => e.Product)
				.WithRequired(e => e.Category)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<Product>()
				.Property(e => e.Price)
				.HasPrecision(19, 4);

			modelBuilder.Entity<Product>()
				.HasMany(e => e.Cart)
				.WithRequired(e => e.Product)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<User>()
				.HasMany(e => e.Cart)
				.WithRequired(e => e.User)
				.WillCascadeOnDelete(false);
		}
	}
}
