using BlackMarket_API.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackMarket_API.Data.Interfaces
{
	public interface ICartRepository
	{
		CartProductsViewModel GetProducts(long userId);
		bool AddProduct(long userId, long productId, int amount = 1);
		ChangeProductAmountViewModel ChangeProductAmount(long userId, long productId, int changeAmountOn);
	}
}
