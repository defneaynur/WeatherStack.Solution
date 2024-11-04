using Core.Framework.Exceptions;
using Dapper;
using System.Data;
using WeatherStack.Library.Models.DatabaseModel;

namespace WeatherStack.Api.Services.Processor.DataProcessor
{
    public interface IFavoriteCityProcessor
    {
        public Task<IEnumerable<FavoriteCities>> GetFavoriteCitiesAsync();
        public Task<FavoriteCities> CreateFavoriteCitiesAsync(FavoriteCities city);
        public Task<bool> DeleteFavoriteCitiesAsync(long id);
        public Task<FavoriteCities> GetFavoriteCityAsync(string city);
    }
    public class FavoriteCityProcessor(IDbConnection _dbConnection, ILogger<FavoriteCityProcessor> _logger) : IFavoriteCityProcessor
    {
        /// <summary>
        /// Veritabanından tüm geçerli (silinmemiş ve 15 dakikadan eski olmayan) favori şehirlerin listesini getiren metot.
        /// </summary>
        /// <returns>Favori şehirlerin listesini içeren <see cref="IEnumerable{FavoriteCities}"/> koleksiyonu.</returns>
        public async Task<IEnumerable<FavoriteCities>> GetFavoriteCitiesAsync()
        { 
            _logger.LogInformation("Veritabanından tüm favori şehirler için GetFavoriteCitiesAsync metodu çağrıldı.");

            const string query = @"SELECT * 
                                    FROM FavoriteCities 
                                    WHERE (IsDeleted IS NULL OR IsDeleted = 0)
                                        AND TIMESTAMPDIFF(MINUTE, LastUpdated, UTC_TIMESTAMP()) < @TimeLimit
                                    ORDER BY LastUpdated";

            var timeLimit = 15; //minute
            var result = await _dbConnection.QueryAsync<FavoriteCities>(query, new { TimeLimit = timeLimit });

            return result;
        }

        /// <summary>
        /// Veritabanından belirtilen şehir için geçerli (silinmemiş ve 15 dakikadan eski olmayan) favori şehir bilgisini getiren metot.
        /// </summary>
        /// <param name="city">Favori şehir olarak getirilecek şehrin adı.</param>
        /// <returns>Favori şehir bilgisi içeren <see cref="FavoriteCities"/> nesnesi. Şehir yoksa null döner.</returns>
        public async Task<FavoriteCities> GetFavoriteCityAsync(string city)
        { 
            _logger.LogInformation("Veritanabından seçilen favori şehir bilgisi için GetFavoriteCityAsync metodu çağrıldı. Şehir: {City}", city);

            const string query = @"SELECT * FROM FavoriteCities 
                                            WHERE CityName = @CityName 
                                                AND (IsDeleted IS NULL OR IsDeleted = 0)
                                                AND TIMESTAMPDIFF(MINUTE, LastUpdated, UTC_TIMESTAMP()) < @TimeLimit
                                            ORDER BY LastUpdated";

            var timeLimit = 15; //minute

            var result = await _dbConnection.QuerySingleOrDefaultAsync<FavoriteCities>(
                query,
                new { CityName = city, TimeLimit = timeLimit }
            );

            return result;
        }

        /// <summary>
        /// Veritabanına yeni bir favori şehir ekler. Eğer şehir zaten mevcutsa hata verir.
        /// </summary>
        /// <param name="city">Eklenecek favori şehir bilgisi.</param>
        /// <returns>Başarıyla eklenen favori şehir bilgisi içeren <see cref="FavoriteCities"/> nesnesi.</returns>
        /// <exception cref="CoreNotificationException">Şehir zaten mevcutsa fırlatılır.</exception>
        public async Task<FavoriteCities> CreateFavoriteCitiesAsync(FavoriteCities city)
        { 
            _logger.LogInformation("Veritabanın favori şehir eklemek için CreateFavoriteCitiesAsync metodu çağrıldı. Şehir: {City}", city.CityName);

            var cityExists = await GetFavoriteCityAsync(city.CityName);

            if (cityExists != null)
            {
                _logger.LogWarning("Eklenmeye çalışılan şehir zaten mevcut: {City}", city.CityName);
                throw new CoreNotificationException("Eklemeye çalıştığınız şehir bulunmaktadır!");
            }

            const string query = @"
                INSERT INTO FavoriteCities (CityName, Temperature, `Condition`, LastUpdated)
                VALUES (@CityName, @Temperature, @Condition, @LastUpdated)"
            ;

            var result = await _dbConnection.ExecuteAsync(query, city);

            return city;
        }

        /// <summary>
        /// Belirtilen ID'ye sahip favori şehri silinmiş olarak güncelleyen metot.
        /// </summary>
        /// <param name="id">Silinecek şehrin ID'si.</param>
        /// <returns>Şehir başarıyla silindiyse true, aksi takdirde false döner.</returns>
        public async Task<bool> DeleteFavoriteCitiesAsync(long id)
        { 
            _logger.LogInformation("Veritabanından favori şehir silmek için DeleteFavoriteCitiesAsync metodu çağrıldı. Şehir ID: {CityId}", id);

            const string query = @"UPDATE FavoriteCities SET IsDeleted = 1 WHERE Id = @Id";

            var result = await _dbConnection.ExecuteAsync(query, new { Id = id });

            return result == 1;
        }
    }
}
