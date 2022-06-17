using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace BlackMarket_API
{
	public class WebApiApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			GlobalConfiguration.Configure(WebApiConfig.Register);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);

			UnityConfig.RegisterComponents();


			//Add these Lines to Serializing Data to JSON Format
			//doesn't work
			//GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
			//GlobalConfiguration.Configuration.Formatters.Remove(GlobalConfiguration.Configuration.Formatters.XmlFormatter);

		}


		//The request failed with code 405, I added this stupid method (it duplicates headers, it's bad)
		//The request began to work. I commented out this method. The request still works...
		//What is going on .Net??
		//protected void Application_BeginRequest(Object sender, EventArgs e)
		//{
			//HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
			//if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
			//{
				//HttpContext.Current.Response.AddHeader("Cache-Control", "no-cache");
				//HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST");
				//HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept");
				//HttpContext.Current.Response.AddHeader("Access-Control-Max-Age", "1728000");
				//HttpContext.Current.Response.End();
			//}
		//}
	}
}
