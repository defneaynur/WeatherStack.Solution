using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace WeatherStack.Api.Base
{
    public static class AppHost
    {
        public static void BaseConfigure(this WebApplicationBuilder builder)
        {
            #region Origin Configuration
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
            #endregion

            #region Serial Log Database Configuration

            #region Hata kontrolü
            //Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine(msg));  // Konsola yazdırma
            //Serilog.Debugging.SelfLog.Enable(msg => System.IO.File.AppendAllText("serilog-errors.txt", msg));  // Dosyaya yazdırma
            #endregion
            var connectionString = builder.Configuration.GetSection("ConfigProject:ApiInformations:ConnectionStrings:DefaultConnection").Value;

            Log.Logger = new LoggerConfiguration() 
              .MinimumLevel.Information() 
              .WriteTo.Console()
              .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
              .WriteTo.MySQL(connectionString: connectionString, tableName: "Logs")
              .CreateLogger();

            builder.Host.UseSerilog();
            #endregion

        }
    }
}
