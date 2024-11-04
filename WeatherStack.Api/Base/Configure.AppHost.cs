using Serilog;

namespace WeatherStack.Api.Base
{
    public static class AppHost
    {
        public static void BaseConfigure(this WebApplicationBuilder builder)
        {
            var allowedOrigin = builder.Configuration.GetSection("ConfigProject:ApiInformations:AllowedOrigins").Get<string[]>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("myAppCors", policy =>
                {
                    policy.WithOrigins("https://localhost:7092")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                });
            });

        }
    }
}
