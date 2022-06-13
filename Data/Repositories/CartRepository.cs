using BlackMarket_API.Data.Interfaces;
using BlackMarket_API.Data.Models;
using BlackMarket_API.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace BlackMarket_API.Data.Repositories
{
	public class CartRepository : ICartRepository
	{
		public CartProductsViewModel GetProducts(long userId)
		{
			using (BlackMarket context = new BlackMarket())
			{
				var cartProducts = context.Cart
					.Where(cart => cart.UserId == userId)
					.Join(context.Product,
					cart => cart.ProductId,
					product => product.ProductId,
					(cart, product) => new CartProductViewModel() { Product = product, Amount = cart.Amount })
					.ToList();

				return new CartProductsViewModel
				{
					Products = cartProducts,
					TotalPrice = cartProducts.Sum(cartProduct => cartProduct.Amount * cartProduct.Product.Price)
				};
			}
		}

		public bool AddProduct(long userId, long productId, int amount = 1)
		{
			using (BlackMarket context = new BlackMarket())
			{
				Cart cart = new Cart() { UserId = userId, ProductId = productId, Amount = amount };
				context.Cart.Add(cart);

				//If the item is already in cart
				try
				{
					context.SaveChanges();
				}
				catch (Exception)
				{
					return false;
				}

				return true;
			}
		}

		public ChangeProductAmountViewModel ChangeProductAmount(long userId, long productId, int changeAmountOn)
		{
			using (BlackMarket context = new BlackMarket())
			{
				ChangeProductAmountViewModel changeProductAmountVM = new ChangeProductAmountViewModel() { ProductId = productId };
				
				Cart modifiedCart = context.Cart.Where(cart => cart.UserId == userId && cart.ProductId == productId).FirstOrDefault();
				if (modifiedCart == null)
					return null;

				if(modifiedCart.Amount + changeAmountOn > 0)
				{
					modifiedCart.Amount += changeAmountOn;
					context.SaveChanges();

					decimal productPrice = context.Product
						.Where(product => product.ProductId == modifiedCart.ProductId)
						.First().Price;

					changeProductAmountVM.AmountChanged = true;
					changeProductAmountVM.TotalPriceChange = changeAmountOn * productPrice;
				}
				else
				{
					changeProductAmountVM.AmountChanged = false;
					changeProductAmountVM.TotalPriceChange = 0;
				}

				changeProductAmountVM.NewAmount = modifiedCart.Amount;


				return changeProductAmountVM;
			}
		}
	}
}