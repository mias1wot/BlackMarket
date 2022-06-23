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
        public IHttpActionResult Add()
		{
			var httpRequest = HttpContext.Current.Request;
            bool hasSliderNumber = false;
            int sliderNumber = 0;
			if (httpRequest.Files.Count < 1)
			{
				return BadRequest("No slider photo was given");
			}
            if (httpRequest.Form["sliderNumber"] != null)
            {
                if (Int32.TryParse(httpRequest.Form["sliderNumber"], out sliderNumber))
                    hasSliderNumber = true;
                else
                    return BadRequest("'sliderNumber' property must be integral number (or be absent)");
            }

            HttpPostedFile photo = httpRequest.Files[0];

			string photoName = photo.FileName;
			Stream photoStream = photo.InputStream;

            string error = sliderRepository.AddSlider(photoName, photoStream, hasSliderNumber ? sliderNumber : (int?)null);
            if (error != null)
                return BadRequest(error);

            return Ok();
        }


        public IHttpActionResult Change()
        {
            var httpRequest = HttpContext.Current.Request;
            int sliderNumber = 0;
            bool hasNewSliderNumber = false;
            int newSliderNumber = 0;
            if (httpRequest.Files.Count < 1)
            {
                return BadRequest("No slider photo was given");
            }


            if (httpRequest.Form["sliderNumber"] != null)
            {
                if (!Int32.TryParse(httpRequest.Form["sliderNumber"], out sliderNumber))
                    return BadRequest("'sliderNumber' property must be integral number");
            }
            else
                return BadRequest("'sliderNumber' property hasn't been passed");


            if (httpRequest.Form["newSliderNumber"] != null)
            {
                if (Int32.TryParse(httpRequest.Form["newSliderNumber"], out newSliderNumber))
                    hasNewSliderNumber = true;
                else
                    return BadRequest("'newSliderNumber' property must be integral number (or be absent)");
            }

            HttpPostedFile photo = httpRequest.Files[0];

            string photoName = photo.FileName;
            Stream photoStream = photo.InputStream;

            string error = sliderRepository.ChangeSlider(sliderNumber, photoName, photoStream, hasNewSliderNumber ? newSliderNumber : (int?)null);
            if (error != null)
                return BadRequest(error);

            return Ok();
        }


        public IHttpActionResult Delete(int sliderNumber)
		{
            string error = sliderRepository.DeleteSlider(sliderNumber);
            if (error != null)
                return BadRequest(error);

            return Ok();
        }


        public IHttpActionResult ChangeSlidersOrder(List<int> sliderNumbers)
        {
            string error = sliderRepository.ChangeSlidersOrder(sliderNumbers);
            if (error != null)
                return BadRequest(error);

            return Ok();
        }
    }
}
