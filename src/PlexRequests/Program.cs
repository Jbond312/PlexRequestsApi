using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace PlexRequests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(
                    builder =>
                    {
                        builder.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Trace);
                        builder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Warning);
                    })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureAppConfiguration(
                    builder =>
                    {
                        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                        builder
                            .AddJsonFile("appsettings.json", true, true)
                            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
                            .AddEnvironmentVariables()
                            .AddEnvironmentVariables("APPSETTING_")
                            .Build();
                    });
    }
}