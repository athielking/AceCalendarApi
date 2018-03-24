using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AssetCalendarApi.Data;
using Microsoft.Extensions.DependencyInjection;

namespace AssetCalendarApi
{
    public class Program
    {
        #region Private Methods

        private static void SeedDatabase(IWebHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                try
                {
                    var seeder = scope.ServiceProvider.GetService<AssetCalendarSeeder>();
                    seeder.Seed().Wait();
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred seeding the DB.");
                }
            }
        }

        #endregion

        #region Public Methods

        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);
       
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (environment == EnvironmentName.Development)
                SeedDatabase(host);

            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();

        #endregion
    }
}
