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
			using (var transaction = context.Database.BeginTransaction())
			{
				//Gets SliderNumber of new Slider
				int actualSliderNumber;
				int? lastSliderNumber = context.Slider.OrderByDescending(Slider => Slider.SliderNumber).Select(Slider => (int?)Slider.SliderNumber).FirstOrDefault();
				if (sliderNumber == null || lastSliderNumber == null || sliderNumber > lastSliderNumber)
				{
					//ignore user input of 'sliderNumber' and sliderNumber will be the last one available
					actualSliderNumber = 1 + (lastSliderNumber ?? 0);
				}
				else
				{
					actualSliderNumber = (int)sliderNumber;

					//Shifts SliderNumber of all sliders
					context.Database.ExecuteSqlCommand(
						$"UPDATE dbo.{nameof(Slider)} " +
						$"SET {nameof(Slider.SliderNumber)} = {nameof(Slider.SliderNumber)} + 1 " +
						$"WHERE SliderNumber >= {sliderNumber}");
				}
				//return null;

				//Uploads photo to Azure Storage
				bool success = AzureStorage.UploadPhotoToAzureStorage(_containerName, photoName, newPhoto, false);
				if (!success)
					return "Failed to upload photo to Azure Storage (maybe this photo is already there or this photo name is already taken)";

				//Adds slider to DB
				Slider slider = new Slider() { SliderNumber = actualSliderNumber, PhotoPath = photoName };
				context.Slider.Add(slider);
				context.SaveChanges();

				transaction.Commit();

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
				Slider changedSlider = context.Slider.SingleOrDefault(Slider => Slider.SliderNumber == sliderNumber);
				if (changedSlider == null)
					return $"There is no Slider with sliderNumber = {sliderNumber}";


				//Shifts sliders sliderNumber accordingly to new sliderNumber
				//User input of 'sliderNumber' is counted only if it's in available sliderNumber range
				var lastSliderNumber = context.Slider.OrderByDescending(Slider => Slider.SliderNumber).FirstOrDefault()?.SliderNumber;
				var oldSliderNumber = changedSlider.SliderNumber;
				if (newSliderNumber != null && newSliderNumber != oldSliderNumber && (lastSliderNumber == null || newSliderNumber <= lastSliderNumber))
				{
					//temporarily changes sliderNumber of changed slider to 0 in sliderNumber to shift other sliders
					changedSlider.SliderNumber = 0;
					context.SaveChanges();

					//Shifts SliderNumber of all sliders between the old and new orders
					char operation;//increment or decrement
					List<int> shiftedSliderNumbers;
					if (newSliderNumber > oldSliderNumber)
					{
						shiftedSliderNumbers = context.Slider.Where(Slider => Slider.SliderNumber > oldSliderNumber && Slider.SliderNumber <= newSliderNumber).Select(Slider => Slider.SliderNumber).ToList();
						operation = '-';
					}
					else
					{
						shiftedSliderNumbers = context.Slider.Where(Slider => Slider.SliderNumber >= newSliderNumber && Slider.SliderNumber < oldSliderNumber).Select(Slider => Slider.SliderNumber).ToList();
						operation = '+';
					}
					var shiftedSliderNumbersStr = String.Join(", ", shiftedSliderNumbers.ToArray());

					//Shifts SliderNumber of all sliders between the old and new orders
					context.Database.ExecuteSqlCommand(
						$"UPDATE dbo.{nameof(Slider)} " +
						$"SET {nameof(Slider.SliderNumber)} = {nameof(Slider.SliderNumber)} {operation} 1 " +
						$"WHERE SliderNumber IN ({shiftedSliderNumbersStr})");

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
			using(var transaction = context.Database.BeginTransaction())
			{
				//Gets slider for deletion
				Slider deletionSlider = context.Slider.SingleOrDefault(Slider => Slider.SliderNumber == sliderNumber);
				if (deletionSlider == null)
					return $"There is no Slider with sliderNumber = {sliderNumber}";


				//Deletes slider from DB
				context.Slider.Remove(deletionSlider);
				context.SaveChanges();


				//Shifts SliderNumber of all sliders going after the deleted slider
				var deletedSliderNumber = deletionSlider.SliderNumber;
				context.Database.ExecuteSqlCommand(
					$"UPDATE dbo.{nameof(Slider)} " +
					$"SET {nameof(Slider.SliderNumber)} = {nameof(Slider.SliderNumber)} - 1 " +
					$"WHERE {nameof(Slider.SliderNumber)} > {deletedSliderNumber}");


				//Deletes photo from Azure Storage
				AzureStorage.DeletePhotoInAzureStorage(_containerName, deletionSlider.PhotoPath);


				transaction.Commit();

				return null;
			}
		}


		//Deletes sliders from DB and their photos from Azure Storage
		//returns error message or null
		public string DeleteAllSliders()
		{
			using (BlackMarket context = new BlackMarket())
			{
				//Gets slider for deletion
				var sliders = context.Slider.ToList();
				
				//Deletes all Sliders from DB
				context.Database.ExecuteSqlCommand($"DELETE FROM dbo.{nameof(Slider)}");

				//Deletes photo from Azure Storage
				foreach(Slider slider in sliders)
				{
					AzureStorage.DeletePhotoInAzureStorage(_containerName, slider.PhotoPath);
				}


				return null;
			}
		}

		//Changed SliderNumber of sliders accordingly to passed sliderNumbers
		//The order of SliderNumbers is the new order of slider images
		//For example, {3,2,1} means that the third slider becomes the first one and the first slider becomes the third one
		//returns error message or null
		public string ChangeSlidersOrder(List<int> sliderNumbers)
		{
			if (sliderNumbers == null)
				return "No list of sliderNumbers was given";

			using (BlackMarket context = new BlackMarket())
			using(var transaction = context.Database.BeginTransaction())
			{
				//Gets slider from DB
				List<Slider> sliders = context.Slider.ToList();

				if (sliderNumbers.Count != sliders.Count)
					return "Quantity of sliders in the provided list does not matche quantity of sliders in database";


				//negatives the SliderNumber of all Sliders so that there is no two identical SliderNumbers (request of unique clustered index)
				context.Database.ExecuteSqlCommand(
					$"UPDATE {nameof(Slider)} " +
					$"SET {nameof(Slider.SliderNumber)} = - {nameof(Slider.SliderNumber)}");


				//Shifts SliderNumber of all sliders
				//Finds the new order of sliders
				List<Slider> slidersInNewOrder = new List<Slider>();
				foreach (int sliderNumber in sliderNumbers)
				{
					Slider changedSlider = sliders.SingleOrDefault(slider => slider.SliderNumber == sliderNumber);
					if (changedSlider == null)
						return $"Slider with sliderNumber = { sliderNumber } isn't found in the database";

					slidersInNewOrder.Add(changedSlider);
				}

				//Put sliders into correct order
				int curOrder = 1;
				foreach (Slider slider in slidersInNewOrder)
				{
					slider.SliderNumber = curOrder++;

					//This is needed because some sliders haven't changed their position but they'd changed SliderNumber to the negative one before to avoid collisions.
					//So this will inforce to return back the correct SliderNumber
					context.Entry(slider).State = System.Data.Entity.EntityState.Modified;
				}
				context.SaveChanges();


				transaction.Commit();

				return null;
			}
		}
	}
}