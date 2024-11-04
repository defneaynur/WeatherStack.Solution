using System.ComponentModel;

namespace WeatherStack.Library.Models.ApiModel.WeatherApi.RequestModel
{
    public class WeatherApiRequest
    {
        public string? key { get; set; }
        [Description("Query")]
        public string q { get; set; }
        public int days { get; set; } = 1;
        public string aqi { get; set; } = "no";
        public string alerts { get; set; } = "no";
    }
}
