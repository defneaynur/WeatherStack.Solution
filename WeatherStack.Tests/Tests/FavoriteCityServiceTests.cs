using Core.Framework.Exceptions;
using Core.Framework.Response;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherStack.Api.Services;
using WeatherStack.Api.Services.Processor.DataProcessor;
using WeatherStack.Library.Models.DatabaseModel;

public class FavoriteCityServiceTests
{
    private readonly Mock<IFavoriteCityProcessor> _favoriteCityProcessorMock;
    private readonly Mock<ILogger<FavoriteCityService>> _loggerMock;
    private readonly FavoriteCityService _controller;

    public FavoriteCityServiceTests()
    {
        _favoriteCityProcessorMock = new Mock<IFavoriteCityProcessor>();
        _loggerMock = new Mock<ILogger<FavoriteCityService>>();

        _controller = new FavoriteCityService(
            _favoriteCityProcessorMock.Object,
            _loggerMock.Object
        );
    }
    /// <summary>
    /// Favori şehirlerin başarıyla döndüğünü doğrular.
    /// Veritabanında bulunan şehirler için <see cref="CoreResponseCode.Success"/> bekler.
    /// </summary>
    [Fact]
    public async Task GetFavoriteCities_ShouldReturnCities_WhenCitiesExist()
    {
        var favoriteCities = new List<FavoriteCities>
        {
            new FavoriteCities { CityName = "Istanbul", Temperature = 15.4, Condition = "Cloudy" },
            new FavoriteCities { CityName = "Ankara", Temperature = 20.5, Condition = "Clear" }
        };

        _favoriteCityProcessorMock
            .Setup(x => x.GetFavoriteCitiesAsync())
            .ReturnsAsync(favoriteCities);

        var result = await _controller.GetFavoriteCities();

        Assert.NotNull(result);
        Assert.Equal(CoreResponseCode.Success, result.ResponseCode);
        Assert.Equal(2, result.Data.Count());
    }

    /// <summary>
    /// Yeni bir favori şehir başarıyla eklendiğinde, başarılı yanıt döndüğünü doğrular.
    /// <see cref="CoreResponseCode.Success"/> yanıtı ve doğru log mesajını bekler.
    /// </summary>
    [Fact]
    public async Task CreateFavoriteCities_ShouldReturnSuccess_WhenCityIsAdded()
    {
        var newCity = new FavoriteCities { CityName = "Mersin", Temperature = 25.5, Condition = "Sunny" };

        _favoriteCityProcessorMock
            .Setup(x => x.CreateFavoriteCitiesAsync(newCity))
            .ReturnsAsync(newCity);

        var result = await _controller.CreateFavoriteCities(newCity);

        Assert.NotNull(result);
        Assert.Equal(CoreResponseCode.Success, result.ResponseCode);
        Assert.Equal("Favori şehir başarıyla eklendi.", result.Message);

        _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CreateFavoriteCities metodu çağrıldı")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

    }

    /// <summary>
    /// Belirtilen şehir ID'si ile favori şehir başarıyla silindiğinde, başarılı yanıt döndüğünü doğrular.
    /// <see cref="CoreResponseCode.Success"/> ve doğru log mesajını bekler.
    /// </summary>
    [Fact]
    public async Task DeleteFavoriteCities_ShouldReturnSuccess_WhenCityIsDeleted()
    {
        var cityId = 1;

        _favoriteCityProcessorMock
            .Setup(x => x.DeleteFavoriteCitiesAsync(cityId))
            .ReturnsAsync(true);

        var result = await _controller.DeleteFavoriteCities(cityId);

        Assert.NotNull(result);
        Assert.Equal(CoreResponseCode.Success, result.ResponseCode);
        Assert.True(result.Data);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DeleteFavoriteCities metodu çağrıldı. ")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Eklenmek istenen şehir zaten favorilerde bulunduğunda uyarı yanıtı döndüğünü doğrular.
    /// <see cref="CoreResponseCode.Info"/> yanıtı ve doğru log mesajını bekler.
    /// </summary>
    [Fact]
    public async Task CreateFavoriteCities_ShouldReturnInfo_WhenCityAlreadyExists()
    {
        var existingCity = new FavoriteCities { CityName = "Istanbul" };

        _favoriteCityProcessorMock
            .Setup(x => x.CreateFavoriteCitiesAsync(existingCity))
            .ThrowsAsync(new CoreNotificationException("Eklemeye çalıştığınız şehir bulunmaktadır!"));

        var result = await _controller.CreateFavoriteCities(existingCity);

        Assert.NotNull(result);
        Assert.Equal(CoreResponseCode.Info, result.ResponseCode);
        Assert.Equal("Eklemeye çalıştığınız şehir bulunmaktadır!", result.Message);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Favori şehir eklenirken bir uyarı oluştu:")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
