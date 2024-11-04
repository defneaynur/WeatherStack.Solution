using Core.Framework.Response;
using Microsoft.AspNetCore.Mvc;
using WeatherStack.Api.Services.Processor.DataProcessor;
using WeatherStack.Api.Services.Processor.WeatherProcessor;
using WeatherStack.Library.Models.ApiModel.Base;
using WeatherStack.Library.Models.ApiModel.WeatherApi.RequestModel;
using Microsoft.Extensions.Logging;

namespace WeatherStack.Api.Services
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherService(IWeatherApiProcessor _weatherApiProcessor, IFavoriteCityProcessor _favoriteCityProcessor, ILogger<WeatherService> _logger) : Controller
    {

        [HttpPost]
        public async Task<CoreResponse<WeatherBaseModel>> GetWeather([FromBody] WeatherApiRequest apiRequest)
        {
            _logger.LogInformation("GetWeather metodu çağrıldı: Şehir={City}", apiRequest.q);

            var result = await _weatherApiProcessor.GetWeatherApiInfoAsync(apiRequest);

            if (result == null)
            {
                _logger.LogWarning("{City} şehri için hava durumu bilgisi bulunamadı:", apiRequest.q);
                return new CoreResponse<WeatherBaseModel>
                {
                    Data = null,
                    ResponseCode = CoreResponseCode.NoData,
                    ErrorMessages = new List<string>(),
                    Message = "Hava durumu bilgisi bulunamadı."
                };
            }

            _logger.LogInformation("Hava durumu başarıyla getirildi: Şehir={City}, Sıcaklık={Temperature}", apiRequest.q, result.Temperature);
            return new CoreResponse<WeatherBaseModel>
            {
                Data = result,
                ResponseCode = CoreResponseCode.Success,
                ErrorMessages = new List<string>(),
                Message = ""
            };
        }
    }

}

