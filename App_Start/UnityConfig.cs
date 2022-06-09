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


            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}