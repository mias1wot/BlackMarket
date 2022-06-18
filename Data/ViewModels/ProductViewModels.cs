using BlackMarket_API.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static BlackMarket_API.Attributes.EnhancedAutomapperAttributes;
using BlackMarket_API.Attributes;

namespace BlackMarket_API.Data.ViewModels
{
	public class TestProductViewModel
	{
		public long ProductId { get; set; }
		public string Name { get; set; }


		[EnhancedAutomapperAttributes.FormedFrom(DbFieldName = nameof(Product.PhotoPath))]
		public byte[] Photo { get; set; }


		//[EnhancedAutomapperAttributes.GroupJoin(nameof(Cart), nameof(Product.ProductId), nameof(Cart.ProductId),
		//	GroupJoin.Operations.Sum, "Amount")]

		[EnhancedAutomapperAttributes.GroupJoin(nameof(Cart), nameof(Product.ProductId), nameof(Cart.ProductId))]
		[EnhancedAutomapperAttributes.GroupJoin.ActionOverInnerCollection(GroupJoin.Operations.Sum, "Amount", typeof(TestProductViewModel))]
		public int SoldAmount { get; set; }


		[EnhancedAutomapperAttributes.GroupJoin(nameof(Cart), nameof(Product.ProductId), nameof(Cart.ProductId))]
			//new List<EnhancedAutomapperOperations>() { (GroupJoin.Operations.Select, "UserId"), (GroupJoin.Operations.Contains, "PassedParam::userId") })]
		[EnhancedAutomapperAttributes.GroupJoin.ActionOverInnerCollection(GroupJoin.Operations.Select, "UserId", typeof(TestProductViewModel))]	
		
		//Warning! This has passed parameter - the parameter you need to pass into method and use
		[EnhancedAutomapperAttributes.GroupJoin.ActionOverInnerCollection(GroupJoin.Operations.Contains, "PassedParam::userId", typeof(TestProductViewModel))]	
		public bool InCart { get; set; }
	}



	public class ProductsViewModel
	{
		public IEnumerable<ProductViewModel> Products { get; set; }
	}

	public class ProductViewModel
	{
		public long ProductId { get; set; }
		public string Name { get; set; }
		public decimal Price { get; set; }
		public int CategoryId { get; set; }
		public string Description { get; set; }
		public string ExtraDescription { get; set; }


		public byte[] Photo { get; set; }
		public int SoldAmount { get; set; }
		public bool InCart { get; set; }
	}

	public class CartProductsViewModel
	{
		public IEnumerable<CartProductViewModel> Products { get; set; }
		public decimal TotalPrice { get; set; }
	}

	public class CartProductViewModel
	{
		public Product Product { get; set; }
		public int Amount { get; set; }
	}

	public class ChangeProductAmountViewModel
	{
		public long ProductId { get; set; }
		public int NewAmount { get; set; }
		public bool AmountChanged { get; set; }
		public decimal TotalPriceChange { get; set; }
	}
}