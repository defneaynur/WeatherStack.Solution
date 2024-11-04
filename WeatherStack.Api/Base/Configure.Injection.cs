using Core.Config.Config;
using Core.Config.Injection;
using MySqlConnector;
using StackExchange.Redis;
using System.Data;
using WeatherStack.Api.Services.Processor.CacheProcessor;
using WeatherStack.Api.Services.Processor.DataProcessor;
using WeatherStack.Api.Services.Processor.WeatherProcessor;

namespace WeatherStack.Api.Base;

public static class ConfigureInjection
{
    private static readonly IBaseInjection _baseInjection;
    public static void BaseInject(this WebApplicationBuilder builder)
    {
        builder.BaseDefaultInjection();

        var config = builder.GetConfigFromAppSettings<ConfigProject>();
        //DB injection
        builder.Services.AddScoped<IDbConnection>(sp =>
            new MySqlConnection(config.ApiInformations.ConnectionStrings.DefaultConnection));

        //Redis Inject
        builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(config.ApiInformations.Redis.Configuration));

        #region Base Injections
        builder.Services.AddScoped<IWeatherApiProcessor, WeatherApiProcessor>();
        builder.Services.AddScoped<IFavoriteCityProcessor, FavoriteCityProcessor>();
        builder.Services.AddScoped<IWeatherCacheProcessor, WeatherCacheProcessor>();
        #endregion

    }

}
