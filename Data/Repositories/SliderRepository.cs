using BlackMarket_API.Data.Interfaces;
using BlackMarket_API.Data.Models;
using BlackMarket_API.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace BlackMarket_API.Data.Repositories
{
	public class SliderRepository: ISliderRepository
	{
		const string _containerName = "sliders";


		public SliderViewModel GetSliders()
		{
			using(BlackMarket context = new BlackMarket())
			{
				var sliders = context.Slider.OrderBy(slider => slider.Order).Select(slider => slider.PhotoPath).ToList();
				var photos = AzureStorage.GetPhotosFromAzureStorage(_containerName, sliders);
				return new SliderViewModel
				{
					Photos = photos.Select(photo => new SliderPhoto { Photo = photo })
				};
			}
		}

		public string AddSlider(string photoName, Stream newPhoto, int order = 0)
		{
			if (order < 0)
				return "Order cannot be less than 0";

			using (BlackMarket context = new BlackMarket())
			{
				//Gets Order of new Slider
				int actualSliderOrder = order;
				if (order == 0)
				{
					//Gets the last available Order
					actualSliderOrder = 1 + context.Slider.OrderBy(Slider => Slider.Order).Last().Order;
				}
				else
				{
					//Shifts Order of all sliders
					var changedSliders = context.Slider.Where(Slider => Slider.Order >= order).ToList();
					changedSliders.ForEach(changedSlider => changedSlider.Order += 1);
					//Saving goes later
				}

				//Uploads photo to Azure Storage
				bool success = AzureStorage.UploadPhotoToAzureStorage(_containerName, photoName, newPhoto, false);
				if (!success)
					return "Cannot upload to Azure Storage";

				//Adds slider to DB
				Slider slider = new Slider() { Order = actualSliderOrder, PhotoPath = photoName };
				context.Slider.Add(slider);
				context.SaveChanges();

				return null;
			}
		}
	}
}