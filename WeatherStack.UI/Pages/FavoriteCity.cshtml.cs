using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WeatherStack.UI.Pages
{
    public class FavoriteCityModel : PageModel
    {
        private readonly ILogger<FavoriteCityModel> _logger;

        public FavoriteCityModel(ILogger<FavoriteCityModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }

}
