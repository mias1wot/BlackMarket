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
	public class SliderRepository : ISliderRepository
	{
		const string _containerName = "sliders";


		public SliderViewModel GetSliders()
		{
			using (BlackMarket context = new BlackMarket())
			{
				var sliders = context.Slider.OrderBy(slider => slider.SliderNumber).Select(slider => slider.PhotoPath).ToList();
				var photos = AzureStorage.GetPhotosFromAzureStorage(_containerName, sliders);
				return new SliderViewModel
				{
					Photos = photos.Select(photo => new SliderPhoto { Photo = photo })
				};
			}
		}

		//returns error message or null
		public string AddSlider(string photoName, Stream newPhoto, int? sliderNumber = null)
		{
			if (sliderNumber <= 0)
				return "'sliderNumber' must be greater than 0 (or be absent)";

			using (BlackMarket context = new BlackMarket())
			{
				//Gets SliderNumber of new Slider
				int actualSliderNumber;
				var lastSliderNumber = context.Slider.OrderByDescending(Slider => Slider.SliderNumber).FirstOrDefault()?.SliderNumber;
				if (sliderNumber == null || lastSliderNumber == null || sliderNumber > lastSliderNumber)
				{
					//ignore user input of 'sliderNumber' and sliderNumber will be the last one available
					actualSliderNumber = 1 + (lastSliderNumber ?? 0);
				}
				else
				{
					actualSliderNumber = (int)sliderNumber;

					//Shifts SliderNumber of all sliders
					//OrderByDescending to change sliderNumber from the end so that there is no two identical sliderNumber in each time
					//var changedSliders = context.Slider.Where(Slider => Slider.SliderNumber >= sliderNumber).OrderByDescending(Slider => Slider.SliderNumber).ToList();
					var changedSliders = context.Slider.Where(Slider => Slider.SliderNumber >= sliderNumber).ToList();
					changedSliders.ForEach(changedSlider => changedSlider.SliderNumber += 1);
					//Saving goes later
				}

				//Uploads photo to Azure Storage
				bool success = AzureStorage.UploadPhotoToAzureStorage(_containerName, photoName, newPhoto, false);
				if (!success)
					return "Failed to upload photo to Azure Storage (maybe this photo is already there or this photo name is already taken)";

				//Adds slider to DB
				context.SaveChanges();

				Slider slider = new Slider() { SliderNumber = actualSliderNumber, PhotoPath = photoName };
				context.Slider.Add(slider);
				context.SaveChanges();

				return null;
			}
		}

		//if 'newSliderNumber' equals to 0, then the SliderNumber won't be changed
		//returns error message or null
		public string ChangeSlider(int sliderNumber, string newPhotoName, Stream newPhoto, int? newSliderNumber = null)
		{
			if (newSliderNumber < 0)
				return "'newSliderNumber' cannot be less than 0";

			using (BlackMarket context = new BlackMarket())
			using(var transaction = context.Database.BeginTransaction())
			{
				//Gets changed slider
				Slider changedSlider = context.Slider.Find(sliderNumber);
				if (changedSlider == null)
					return $"There is no Slider with sliderNumber = {sliderNumber}";


				//Shifts sliders sliderNumber accordingly to new sliderNumber
				//User input of 'sliderNumber' is counted only if it's in available sliderNumber range
				var lastSliderNumber = context.Slider.OrderByDescending(Slider => Slider.SliderNumber).FirstOrDefault()?.SliderNumber;
				if (newSliderNumber != null && (lastSliderNumber == null || newSliderNumber <= lastSliderNumber))
				{
					//temporarily changes sliderNumber of changed slider to 0 in sliderNumber to shift other sliders
					changedSlider.SliderNumber = 0;
					context.SaveChanges();

					//Shifts SliderNumber of all sliders between the old and new orders
					var oldSliderNumber = changedSlider.SliderNumber;
					if (newSliderNumber > oldSliderNumber)
					{
						var shiftedSliders = context.Slider.Where(Slider => Slider.SliderNumber > oldSliderNumber && Slider.SliderNumber <= newSliderNumber).ToList();
						shiftedSliders.ForEach(shiftedSlider => shiftedSlider.SliderNumber -= 1);
						
					}
					else
					{
						var shiftedSliders = context.Slider.Where(Slider => Slider.SliderNumber >= newSliderNumber && Slider.SliderNumber < oldSliderNumber).ToList();
						shiftedSliders.ForEach(shiftedSlider => shiftedSlider.SliderNumber += 1);
					}
					changedSlider.SliderNumber = (int)newSliderNumber;
					//Saving goes later
				}

				//Deletes old photo from Azure Storage
				if (changedSlider.PhotoPath != newPhotoName)
					AzureStorage.DeletePhotoInAzureStorage(_containerName, changedSlider.PhotoPath);

				//Changes photo in Azure Storage
				bool success = AzureStorage.UploadPhotoToAzureStorage(_containerName, newPhotoName, newPhoto, true);
				if (!success)
					return "Failed to upload photo to Azure Storage";

				//Adds slider to DB
				changedSlider.PhotoPath = newPhotoName;
				context.SaveChanges();


				transaction.Commit();

				return null;
			}
		}

		//returns error message or null
		public string DeleteSlider(int sliderNumber)
		{
			using (BlackMarket context = new BlackMarket())
			{
				//Gets slider for deletion
				Slider deletionSlider = context.Slider.Find(sliderNumber);
				if (deletionSlider == null)
					return $"There is no Slider with sliderNumber = {sliderNumber}";


				//Shifts SliderNumber of all sliders going after the slider to be deleted
				var deletedOrder = deletionSlider.SliderNumber;
				var shiftedSliders = context.Slider.Where(Slider => Slider.SliderNumber > deletedOrder).ToList();
				shiftedSliders.ForEach(shiftedSlider => shiftedSlider.SliderNumber -= 1);
				//Saving goes later


				//Deletes photo from Azure Storage
				AzureStorage.DeletePhotoInAzureStorage(_containerName, deletionSlider.PhotoPath);


				//Deletes slider from DB
				context.Slider.Remove(deletionSlider);
				context.SaveChanges();

				return null;
			}
		}

		//Changed SliderNumber of sliders accordingly to sliderNumber of slidersId in 'sliderIds'
		//returns error message or null
		public string ChangeSlidersOrder(List<int> sliderNumbers)
		{
			if (sliderNumbers == null)
				return "No list of sliderNumbers was given";

			using (BlackMarket context = new BlackMarket())
			{
				//Gets slider from DB
				List<Slider> sliders = context.Slider.ToList();

				if (sliderNumbers.Count != sliders.Count)
					return "Quantity of sliders in the provided list does not matche quantity of sliders in database";


				//Shifts SliderNumber of all sliders
				int curOrder = 1;
				foreach (int sliderNumber in sliderNumbers)
				{
					Slider changedSlider = sliders.Find(slider => slider.SliderNumber == sliderNumber);
					if (changedSlider == null)
						return $"Slider with sliderNumber = { sliderNumber } isn't found in the database";

					changedSlider.SliderNumber = curOrder++;
				}

				context.SaveChanges();

				return null;
			}
		}
	}
}