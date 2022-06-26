using AutoMapper;
using BlackMarket_API.Data.Models;
using BlackMarket_API.Data.Services.Interfaces;
using BlackMarket_API.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace BlackMarket_API.Data.Services.Repositories
{
	public class CartRepository : ICartRepository
	{
		const string _productsContainerName = "products";


		public CartProductsViewModel GetProducts(long userId, IMapper mapper)
		{
			if (userId == 0)
				return null;

			using (BlackMarket context = new BlackMarket())
			{
				var cartProducts = context.Cart
					.Where(cart => cart.UserId == userId)
					.Join(context.Product,
					cart => cart.ProductId,
					product => product.ProductId,
					(cart, product) => new { Product = product, cart.Amount })
					.ToList();

				//Gets ViewModels of cart products
				var cartProductVMList = cartProducts.Select(cartProduct =>
				{
					CartProductViewModel cartProductVM = mapper.Map<CartProductViewModel>(cartProduct.Product);
					cartProductVM.Amount = cartProduct.Amount;
					return cartProductVM;
				}).ToList();

				//Gets photos from Azure
				var photos = AzureStorage.GetPhotosFromAzureStorage(_productsContainerName, cartProducts.Select(cartProduct => cartProduct.Product.PhotoPath).ToList());
				var photoIterator = photos.GetEnumerator();
				cartProductVMList.ForEach(cartProductVM =>
				{
					photoIterator.MoveNext();
					cartProductVM.Photo = photoIterator.Current;
				});

				return new CartProductsViewModel
				{
					Products = cartProductVMList,
					TotalPrice = cartProducts.Sum(cartProduct => cartProduct.Amount * cartProduct.Product.Price)
				};
			}
		}

		public bool AddProduct(long userId, long productId, int amount = 1)
		{
			if (userId == 0)
				return false;

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
			if (userId == 0)
				return null;

			using (BlackMarket context = new BlackMarket())
			{
				ChangeProductAmountViewModel changeProductAmountVM = new ChangeProductAmountViewModel() { ProductId = productId };

				Cart modifiedCart = context.Cart.Where(cart => cart.UserId == userId && cart.ProductId == productId).FirstOrDefault();
				if (modifiedCart == null)
					return null;

				if (modifiedCart.Amount + changeAmountOn > 0)
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

		//returns either error message or null
		public string DeleteProduct(long userId, long productId)
		{
			using (BlackMarket context = new BlackMarket())
			{
				Cart cart = context.Cart.Find(userId, productId);
				if (cart == null)
					return $"No product in cart with ProductId = {productId} which belongs to user with UserId = {userId} was found";

				context.Cart.Remove(cart);
				context.SaveChanges();
				return null;
			}
		}
	}
}