using AutoMapper;
using BlackMarket_API.Data.Models;
using BlackMarket_API.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlackMarket_API.App_Start
{
	public static class MapperConfig
	{
		public static IMapper Register()
		{
			var config = new MapperConfiguration(cfg =>
			{
				//Create all maps here
				//cfg.CreateMap<Order, OrderDto>();
				cfg.CreateMap<Product, HomeProductViewModel>();
				cfg.CreateMap<Product, OpenedProductViewModel>();
				cfg.CreateMap<Product, CartProductViewModel>();
				cfg.CreateMap<Category, CategoryViewModel>();
			});

			IMapper mapper = config.CreateMapper();

			return mapper;
		}
	}
}