using Core.Framework.Response;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherStack.Api.Services;
using WeatherStack.Api.Services.Processor.DataProcessor;
using WeatherStack.Api.Services.Processor.WeatherProcessor;
using WeatherStack.Library.Models.ApiModel.Base;
using WeatherStack.Library.Models.ApiModel.WeatherApi.RequestModel;

public class WeatherServiceTests
{
    private readonly Mock<IWeatherApiProcessor> _weatherApiProcessorMock;
    private readonly Mock<IFavoriteCityProcessor> _favoriteCityProcessorMock;
    private readonly Mock<ILogger<WeatherService>> _loggerMock;
    private readonly WeatherService _controller;

    public WeatherServiceTests()
    {
        _weatherApiProcessorMock = new Mock<IWeatherApiProcessor>();
        _favoriteCityProcessorMock = new Mock<IFavoriteCityProcessor>();
        _loggerMock = new Mock<ILogger<WeatherService>>();

        _controller = new WeatherService(
            _weatherApiProcessorMock.Object,
            _favoriteCityProcessorMock.Object,
            _loggerMock.Object
        );
    }
    /// <summary>
    /// Hava durumu bilgisi bulunduğunda başarılı yanıt döndüğünü doğrular.
    /// Verilen şehir için <see cref="CoreResponseCode.Success"/> ve doğru sıcaklık verisini bekler.
    /// </summary>
    [Fact]
    public async Task GetWeather_ShouldReturnSuccess_WhenWeatherDataExists()
    {
        var apiRequest = new WeatherApiRequest { q = "Istanbul" };
        var expectedWeather = new WeatherBaseModel { Temperature = 15.4, City = "Istanbul", Condition = "Cloudy" };

        _weatherApiProcessorMock
            .Setup(x => x.GetWeatherApiInfoAsync(apiRequest))
            .ReturnsAsync(expectedWeather);

        var result = await _controller.GetWeather(apiRequest);

        Assert.NotNull(result);
        Assert.Equal(CoreResponseCode.Success, result.ResponseCode);
        Assert.Equal(expectedWeather.Temperature, result.Data.Temperature);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Hava durumu başarıyla getirildi")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Hava durumu bilgisi bulunamadığında doğru yanıt ve uyarı mesajı döndüğünü doğrular.
    /// Verilen şehir için <see cref="CoreResponseCode.NoData"/> ve uyarı log mesajını bekler.
    /// </summary>
    [Fact]
    public async Task GetWeather_ShouldReturnNoData_WhenWeatherDataIsNull()
    {
        // Arrange
        var apiRequest = new WeatherApiRequest { q = "Mersin" };

        _weatherApiProcessorMock
            .Setup(x => x.GetWeatherApiInfoAsync(apiRequest))
            .ReturnsAsync((WeatherBaseModel)null);

        // Act
        var result = await _controller.GetWeather(apiRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(CoreResponseCode.NoData, result.ResponseCode);
        Assert.Equal("Hava durumu bilgisi bulunamadı.", result.Message);

        // Log doğrulama: eksik veri olduğunda log kontrolü
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("hava durumu bilgisi bulunamadı")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
