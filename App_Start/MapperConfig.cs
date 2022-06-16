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
				cfg.CreateMap<Product, ProductViewModel>();
				//.ForMember(d => d.SoldAmount, opt =>
				//opt.MapFrom(product =>
				//))

			});

			IMapper mapper = config.CreateMapper();

			return mapper;
		}
	}
}