﻿using BlackMarket_API.Data.Interfaces;
using BlackMarket_API.Data.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BlackMarket_API.Controllers
{
	[Authorize]
	public class CartController : ApiController
    {
        readonly ICartRepository cartRepository;
        public CartController(ICartRepository cartRepository)
        {
            this.cartRepository = cartRepository;
        }



        public CartProductsViewModel Get()
		{
            return cartRepository.GetProducts(User.Identity.GetUserId<long>());
		}

        //This is POST method
        public IHttpActionResult Add(long productId)
		{
            if (cartRepository.AddProduct(User.Identity.GetUserId<long>(), productId))
                return Ok();
            return BadRequest("The item is already in cart");
		}

        //The next 3 methods can return null - this means there is no such a product in the cart
        public ChangeProductAmountViewModel IncrementProduct(long productId)
		{
            return ChangeProductAmount(productId, 1);
        }
        public ChangeProductAmountViewModel DecrementProduct(long productId)
		{
            return ChangeProductAmount(productId, -1);
        }

        public ChangeProductAmountViewModel ChangeProductAmount(long productId, int changeAmountOn)
		{
            return cartRepository.ChangeProductAmount(User.Identity.GetUserId<long>(), productId, changeAmountOn);
		}
    }
}
