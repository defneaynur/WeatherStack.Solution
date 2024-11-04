namespace WeatherStack.Library.Models.ApiModel.WeatherStack.RequestModel
{
    public class WeatherStackRequest
    {
        // https://api.weatherstack.com/current?access_key=838c0d5e8fcc1dbbc66e8c1c0a14c6e5&query=Istanbul
        public string access_key { get; set; }
        public string query { get; set; }
    }
}
