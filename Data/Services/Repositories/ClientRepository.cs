using BlackMarket_API.Data.Models;
using BlackMarket_API.Data.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlackMarket_API.Data.Services.Repositories
{
	public class ClientRepository: IClientRepository, IDisposable
	{
		BlackMarket context;
		public ClientRepository()
		{
			context = new BlackMarket();
		}

		public Client GetClient(string clientId)
		{
			return context.Client.Find(clientId);
		}


		public void Dispose()
		{
			context.Dispose();
		}
	}
}