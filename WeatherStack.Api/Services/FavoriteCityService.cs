using Core.Framework.Exceptions;
using Core.Framework.Response;
using Microsoft.AspNetCore.Mvc;
using WeatherStack.Api.Services.Processor.DataProcessor;
using WeatherStack.Library.Models.DatabaseModel;

namespace WeatherStack.Api.Services
{

    [ApiController]
    [Route("api/[controller]")]
    public class FavoriteCityService(IFavoriteCityProcessor _favoriteCityProcessor, ILogger<FavoriteCityService> _logger) : Controller
    {

        [HttpGet] 
        public async Task<CoreResponse<IEnumerable<FavoriteCities>>> GetFavoriteCities()
        {  
            var result = await _favoriteCityProcessor.GetFavoriteCitiesAsync();
            if (result.Count() == 0 || result == null)
            {
                return new CoreResponse<IEnumerable<FavoriteCities>>
                {
                    Data = null,
                    ResponseCode = CoreResponseCode.NoData,
                    ErrorMessages = new List<string>(),
                    Message = "Favori şehir bilgisi bulunamadı."
                };
            }

            return new CoreResponse<IEnumerable<FavoriteCities>>
            {
                Data = result,
                ResponseCode = CoreResponseCode.Success,
                ErrorMessages = new List<string>(),
                Message = ""
            };
        }

        [HttpPost] 
        public async Task<CoreResponse<FavoriteCities>> CreateFavoriteCities(FavoriteCities city)
        { 
            _logger.LogInformation("CreateFavoriteCities metodu çağrıldı. Request: {city}", city);
            try
            {
                var result = await _favoriteCityProcessor.CreateFavoriteCitiesAsync(city);
                _logger.LogInformation("Favori şehir başarıyla eklendi: {CityName}", city.CityName);
                return new CoreResponse<FavoriteCities>
                {
                    Data = result,
                    ResponseCode = CoreResponseCode.Success,
                    Message = "Favori şehir başarıyla eklendi."
                };
            }
            catch (CoreNotificationException ex)
            {
                _logger.LogWarning("Favori şehir eklenirken bir uyarı oluştu: {Message}", ex.Message);
                return new CoreResponse<FavoriteCities>
                {
                    Data = null,
                    ResponseCode = CoreResponseCode.Info,
                    Message = ex.Message
                };
            }
        }

        [HttpPut("delete/{id}")]
        public async Task<CoreResponse<bool>> DeleteFavoriteCities(long id)
        { 
            _logger.LogInformation("DeleteFavoriteCities metodu çağrıldı. Şehir ID: {CityId}", id);
            var result = await _favoriteCityProcessor.DeleteFavoriteCitiesAsync(id);

            if (result)
                _logger.LogInformation("Favori şehir başarıyla silindi. Şehir ID: {CityId}", id);
            else
                _logger.LogWarning("Favori şehir silinemedi. Şehir ID: {CityId}", id);

            return new CoreResponse<bool>
            {
                Data = result,
                ResponseCode = CoreResponseCode.Success,
                ErrorMessages = new List<string>(),
                Message = "Favori şehir silindi."
            };
        }
    }
}
