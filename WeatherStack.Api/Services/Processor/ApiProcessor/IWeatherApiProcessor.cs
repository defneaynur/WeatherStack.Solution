using Core.Config.Injection;
using Core.Framework.Exceptions;
using Newtonsoft.Json;
using WeatherStack.Api.Services.Processor.CacheProcessor;
using WeatherStack.Api.Services.Processor.DataProcessor;
using WeatherStack.Library.Models.ApiModel.Base;
using WeatherStack.Library.Models.ApiModel.WeatherApi.RequestModel;
using WeatherStack.Library.Models.ApiModel.WeatherApi.ResponseModel;
using WeatherStack.Library.Models.ApiModel.WeatherStack.ResponseModel;

namespace WeatherStack.Api.Services.Processor.WeatherProcessor
{
    public interface IWeatherApiProcessor
    {
        public Task<WeatherBaseModel> GetWeatherApiInfoAsync(WeatherApiRequest weatherApi);
    }

    public class WeatherApiProcessor(IBaseInjection _baseInjection, IWeatherCacheProcessor _weatherCacheProcessor, IFavoriteCityProcessor _favoriteCityProcessor, ILogger<WeatherApiProcessor> _logger) : IWeatherApiProcessor
    {
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private static readonly Dictionary<string, List<TaskCompletionSource<WeatherBaseModel>>> pendingRequests = new Dictionary<string, List<TaskCompletionSource<WeatherBaseModel>>>();
        private static readonly TimeSpan maxWaitTime = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Hava durumu bilgisini sırayla Redis, veritabanı ve API üzerinden getirir. 
        /// Öncelikle önbellek (Redis) ve veritabanı kontrol edilir; eğer veri yoksa API'den bilgi alınır ve önbelleğe eklenir.
        /// </summary>
        /// <param name="weatherApi">Hava durumu API isteği içeren parametre.</param>
        /// <returns>Şehir için hava durumu bilgisi içeren <see cref="WeatherBaseModel"/> nesnesi.</returns>
        public async Task<WeatherBaseModel> GetWeatherApiInfoAsync(WeatherApiRequest weatherApi)
        {
            _logger.LogInformation("WeatherApiProcessor/GetWeatherApiInfoAsync metodu çağrıldı. Şehir: {City}", weatherApi.q);

            #region Redis Control
            var cachedWeatherData = await _weatherCacheProcessor.GetCachedWeatherDataAsync(weatherApi.q);
            if (cachedWeatherData != null)
            {
                _logger.LogInformation("Redis önbellekten hava durumu bilgisi alındı: {cachedWeatherData}", cachedWeatherData);
                return cachedWeatherData;
            }
            #endregion

            #region Database Control
            var dbWeatherData = await _favoriteCityProcessor.GetFavoriteCityAsync(weatherApi.q);
            if (dbWeatherData != null)
            {
                _logger.LogInformation("Veritabanından hava durumu bilgisi alındı ve önbelleğe eklendi: {dbWeatherData}", dbWeatherData);
                var weatherData = new WeatherBaseModel
                {
                    City = dbWeatherData.CityName,
                    Temperature = dbWeatherData.Temperature,
                    Condition = dbWeatherData.Condition,
                };
                await _weatherCacheProcessor.CacheWeatherDataAsync(weatherApi.q, weatherData);
                return weatherData;
            }
            #endregion

            string queryKey = weatherApi.q;

            await semaphore.WaitAsync();
            try
            {
                if (!pendingRequests.ContainsKey(queryKey))
                {
                    pendingRequests[queryKey] = new List<TaskCompletionSource<WeatherBaseModel>>();
                    var tcs = new TaskCompletionSource<WeatherBaseModel>();
                    pendingRequests[queryKey].Add(tcs);

                    await Task.Delay(maxWaitTime);

                    var averageWeatherData = await GetWeatherDataAverageAsync(weatherApi);

                    foreach (var tcsItem in pendingRequests[queryKey])
                    {
                        tcsItem.SetResult(averageWeatherData);
                    }

                    #region Redis Cache Register
                    _logger.LogInformation("Redis önbellek güncellemesi. Şehir: {City}", averageWeatherData.City);
                    await _weatherCacheProcessor.CacheWeatherDataAsync(averageWeatherData.City, averageWeatherData);
                    #endregion

                    pendingRequests.Remove(queryKey);
                    return averageWeatherData;
                }
                else
                {
                    var tcs = new TaskCompletionSource<WeatherBaseModel>();
                    pendingRequests[queryKey].Add(tcs);
                    return await tcs.Task;
                }
            }
            finally
            {
                semaphore.Release();
            }
        }


        #region Private Weather Api Requests
        /// <summary>
        /// İki API'den alınan hava durumu bilgilerini karşılaştırarak ortalama sıcaklığı hesaplayan metot.
        /// </summary>
        /// <param name="weatherApi">Hava durumu API isteği içeren model.</param>
        /// <returns>Ortalama hava durumu bilgisi içeren <see cref="WeatherBaseModel"/> nesnesi.</returns>
        private async Task<WeatherBaseModel> GetWeatherDataAverageAsync(WeatherApiRequest weatherApi)
        {
            _logger.LogInformation("Sıcaklık ortalaması için GetWeatherDataAverageAsync metodu çağrıldı. Şehir: {City}", weatherApi.q);

            var weatherApiConfig1 = _baseInjection.ConfigProject.ApiInformations.ConnectedProject.FirstOrDefault(p => p.ProjectId == "WeatherApi");
            var weatherApiConfig2 = _baseInjection.ConfigProject.ApiInformations.ConnectedProject.FirstOrDefault(p => p.ProjectId == "WeatherStack");

            string apiKey1 = weatherApiConfig1.ApiKey;
            string baseUrl1 = weatherApiConfig1.ServiceUri;
            string apiKey2 = weatherApiConfig2.ApiKey;
            string baseUrl2 = weatherApiConfig2.ServiceUri;

            try
            {
                var weatherData = await GetWeatherDataAsync(baseUrl1, apiKey1, weatherApi);
                var weatherStackData = await GetWeatherStackDataAsync(baseUrl2, apiKey2, weatherApi);

                double averageTemperature = (weatherData.current.temp_c + weatherStackData.current.temperature) / 2;

                _logger.LogInformation("Hava durumu ortalaması hesaplandı: {City}, Ortalama Sıcaklık: {Temperature}", weatherApi.q, averageTemperature);

                return new WeatherBaseModel
                {
                    Temperature = averageTemperature,
                    City = weatherApi.q,
                    Condition = weatherData.current.condition.text
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Birinci API başarısız oldu. Yedek API deneniyor: {City}", weatherApi.q);
                try
                {
                    var weatherStackData = await GetWeatherStackDataAsync(baseUrl2, apiKey2, weatherApi);
                    return new WeatherBaseModel
                    {
                        Temperature = weatherStackData.current.temperature,
                        City = weatherApi.q,
                        Condition = weatherStackData.current.weather_descriptions[0]
                    };
                }
                catch (Exception ex2)
                {
                    _logger.LogError(ex2, "Her iki API çağrısı da başarısız oldu: {City}", weatherApi.q);
                    throw new CoreNotificationException("Her iki API çağrısı da başarısız oldu.");
                }
            }
        }

        /// <summary>
        /// Birinci hava durumu API'ye istek yapan ve alınan veriyi dönen metot.
        /// </summary>
        /// <param name="baseUrl">API URL'i.</param>
        /// <param name="apiKey">API anahtarı.</param>
        /// <param name="weatherApi">Hava durumu API isteği parametreleri.</param>
        /// <returns>Hava durumu verisi içeren <see cref="WeatherApiModel"/> nesnesi.</returns>
        private async Task<WeatherApiModel> GetWeatherDataAsync(string baseUrl, string apiKey, WeatherApiRequest weatherApi)
        {
            string url = $"{baseUrl}?key={apiKey}&q={weatherApi.q}&days={weatherApi.days}&aqi={weatherApi.aqi}&alerts={weatherApi.alerts}";
            _logger.LogInformation("Birinci API çağrısı yapılıyor: {Url}", url);

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    WeatherApiModel weatherData = JsonConvert.DeserializeObject<WeatherApiModel>(json);
                    _logger.LogInformation("Birinci API'den veri alındı: {weatherApi}", weatherApi);
                    return weatherData;
                }
                else
                {
                    throw new HttpRequestException($"API çağrısı başarısız oldu: {response.ReasonPhrase}");
                }
            }
        }

        /// <summary>
        /// İkinci hava durumu API'ye istek yapar ve alınan veriyi döner.
        /// </summary>
        /// <param name="baseUrl">API URL'i.</param>
        /// <param name="apiKey">API anahtarı.</param>
        /// <param name="weatherApi">Hava durumu API isteği parametreleri.</param>
        /// <returns>Hava durumu verisi içeren <see cref="WeatherStackApiModel"/> nesnesi.</returns>
        private async Task<WeatherStackApiModel> GetWeatherStackDataAsync(string baseUrl, string apiKey, WeatherApiRequest weatherApi)
        {
            string url = $"{baseUrl}?access_key={apiKey}&query={weatherApi.q}";
            _logger.LogInformation("İkinci API çağrısı yapılıyor: {Url}", url);

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    WeatherStackApiModel weatherData = JsonConvert.DeserializeObject<WeatherStackApiModel>(json);
                    _logger.LogInformation("İkinci API'den veri alındı: {weatherData}", weatherData);
                    return weatherData;
                }
                else
                {
                    throw new HttpRequestException($"API çağrısı başarısız oldu: {response.ReasonPhrase}");
                }
            }
        }
        #endregion

    }


}

