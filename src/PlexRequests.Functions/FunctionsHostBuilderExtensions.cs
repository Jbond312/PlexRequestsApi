using System;
using System.Linq;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PlexRequests.Functions
{
    public static class FunctionsHostBuilderExtensions
    {
        private const string DefaultCurrentDirectory = "/home/site/wwwroot";

        public static IConfiguration AddCustomAppConfiguration(this IFunctionsHostBuilder builder)
        {
            var configuration = BuildConfiguration(builder);

            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), configuration));

            return configuration;
        }

        private static IConfiguration BuildConfiguration(IFunctionsHostBuilder builder)
        {
            var configBuilder = builder.GetBaseConfigurationBuilder();

            var environmentName = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");

            if (string.IsNullOrWhiteSpace(environmentName))
            {
                throw new ArgumentException("Environment variable (AZURE_FUNCTIONS_ENVIRONMENT) is null/empty.", nameof(environmentName));
            }

            configBuilder
#if DEBUG
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
#endif
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .AddEnvironmentVariables("APPSETTING_");

            var finalConfig = configBuilder.Build();

            return finalConfig;
        }

        private static IConfigurationBuilder GetBaseConfigurationBuilder(this IFunctionsHostBuilder builder)
        {
            var configurationBuilder = new ConfigurationBuilder();
            var descriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(IConfiguration));

            if (descriptor?.ImplementationInstance is IConfiguration configRoot)
            {
                configurationBuilder.AddConfiguration(configRoot);
            }

            var rootConfigurationBuilder = configurationBuilder.SetBasePath(GetCurrentDirectory());

            return rootConfigurationBuilder;
        }

        private static string GetCurrentDirectory()
        {
            var currentDirectory = DefaultCurrentDirectory;
            var isLocal = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));

            if (isLocal)
            {
                currentDirectory = Environment.CurrentDirectory;
            }

            return currentDirectory;
        }
    }
}
