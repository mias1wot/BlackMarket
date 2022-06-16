using BlackMarket_API.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlackMarket_API.Data.ViewModels
{
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