using BlackMarket_API.Data.ViewModels;
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
		string AddSlider(string photoName, Stream newPhoto, int order = 0);
	}
}
