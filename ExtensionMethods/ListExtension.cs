using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BlackMarket_API.ExtensionMethods
{
	public static class ListExtension
	{
		public static async Task ForEachAsync<T>(this List<T> list, Func<T, Task> func)
		{
			foreach (T item in list)
			{
				await func(item);
			}
		}
	}
}