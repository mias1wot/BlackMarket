﻿using BlackMarket_API.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackMarket_API.Data.Interfaces
{
	public interface ISliderRepository
	{
		SliderViewModel GetSliders();
		string AddSlider(string photoName, Stream newPhoto, int? sliderNumber = null);
		string ChangeSlider(int sliderNumber, string newPhotoName, Stream newPhoto, int? newSliderNumber = null);
		string DeleteSlider(int sliderNumber);
		string ChangeSlidersOrder(List<int> sliderNumbers);
	}
}
