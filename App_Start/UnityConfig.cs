using BlackMarket_API.App_Start;
using BlackMarket_API.Data.Interfaces;
using BlackMarket_API.Data.Repositories;
using System.Web.Http;
using Unity;
using Unity.WebApi;

namespace BlackMarket_API
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();
            container.RegisterType<IProductRepository, ProductRepository>();
            container.RegisterType<ICategoryRepository, CategoryRepository>();
            container.RegisterType<ICartRepository, CartRepository>();
            container.RegisterType<ISliderRepository, SliderRepository>();

            container.RegisterInstance(MapperConfig.Register()) ;


            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}