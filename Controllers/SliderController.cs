using BlackMarket_API.Data.Interfaces;
using BlackMarket_API.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace BlackMarket_API.Controllers
{
    //[Authorize]
    public class SliderController : ApiController
    {
        readonly ISliderRepository sliderRepository;

        public SliderController(ISliderRepository sliderRepository)
		{
            this.sliderRepository = sliderRepository;
        }



        public SliderViewModel Get()
		{
            return sliderRepository.GetSliders();
        }

        //Slider order begins from 1
        public IHttpActionResult Post()
		{
            


			var httpRequest = HttpContext.Current.Request;
            int sliderOrder = 0;
			if (httpRequest.Files.Count < 1)
			{
				return BadRequest("No slider photo was given");
			}
            if (httpRequest.Form["Order"] != null)
            {
                if (!Int32.TryParse(httpRequest.Form["Order"], out sliderOrder))
                    return BadRequest("Order must be integral number");
            }

            HttpPostedFile photo = httpRequest.Files[0];

			string photoName = photo.FileName;
			Stream photoStream = photo.InputStream;

            string error = sliderRepository.AddSlider(photoName, photoStream, sliderOrder);
            if (error != null)
                return BadRequest(error);

            return Ok();

        }
    }
}
