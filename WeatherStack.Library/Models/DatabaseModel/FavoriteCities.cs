using Newtonsoft.Json;

namespace WeatherStack.Library.Models.DatabaseModel
{
    public class FavoriteCities
    {
        [JsonProperty("Id")]
        public long Id { get; set; }
        [JsonProperty("CityName")]
        public string CityName { get; set; }
        public double Temperature { get; set; }
        public string Condition { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsDeleted { get; set; }
    }
}
