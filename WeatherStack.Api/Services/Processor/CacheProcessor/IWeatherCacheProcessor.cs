using Core.Config.Config;
using Newtonsoft.Json;
using StackExchange.Redis;
using WeatherStack.Library.Models.ApiModel.Base;

namespace WeatherStack.Api.Services.Processor.CacheProcessor
{
    public interface IWeatherCacheProcessor
    {
        public Task CacheWeatherDataAsync(string city, WeatherBaseModel weatherData);
        public Task<WeatherBaseModel?> GetCachedWeatherDataAsync(string city);
    }

    public class WeatherCacheProcessor(IConnectionMultiplexer _redis, IConfigProject _configProject) : IWeatherCacheProcessor
    {
        /// <summary>
        /// Hava durumu bilgisini Redis önbelleğe kaydeden metot.
        /// </summary>
        /// <param name="city">Şehrin adı.</param>
        /// <param name="weatherData">Önbelleğe kaydedilecek hava durumu verisi.</param>
        /// <returns>Görev tamamlandığında <see cref="Task"/> döner.</returns>
        public async Task CacheWeatherDataAsync(string city, WeatherBaseModel weatherData)
        {
            var db = _redis.GetDatabase();
            var serializedData = JsonConvert.SerializeObject(weatherData);
            var waitingTime = _configProject.ApiInformations.Redis.Time;

            await db.StringSetAsync(city, serializedData, TimeSpan.FromMinutes(waitingTime));
        }

        /// <summary>
        /// Redis önbellekte mevcut olan hava durumu bilgisini getiren metot.
        /// </summary>
        /// <param name="city">Önbellekten getirilecek şehrin adı.</param>
        /// <returns>Hava durumu bilgisi içeren <see cref="WeatherBaseModel"/> nesnesi; eğer veri yoksa null döner.</returns>
        public async Task<WeatherBaseModel> GetCachedWeatherDataAsync(string city)
        {
            var db = _redis.GetDatabase();
            var cachedData = await db.StringGetAsync(city);

            if (!cachedData.IsNullOrEmpty)
            {
                return JsonConvert.DeserializeObject<WeatherBaseModel>(cachedData);
            }

            return null;
        }
    }

}
