﻿using System;
using Microsoft.Extensions.DependencyInjection;
using PlexRequests.Api;
using PlexRequests.Plex;
using PlexRequests.Settings;
using PlexRequests.Store;

namespace PlexRequests
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterDependencies(this IServiceCollection services)
        {
            RegisterServices(services);
            RegisterRepositories(services);
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddTransient<IPlexApi, PlexApi>();
            services.AddSingleton<IApiService, ApiService>();
            services.AddTransient<ISettingsService, SettingsService>();
            services.AddSingleton<IPlexRequestsHttpClient, PlexRequestsHttpClient>();
        }

        private static void RegisterRepositories(IServiceCollection services)
        {
            var databaseName = Environment.GetEnvironmentVariable(EnvironmentVariableKeys.MongoDatabaseName);
            var server = Environment.GetEnvironmentVariable(EnvironmentVariableKeys.MongoServerName);

            var connectionString = $"mongodb://{server}:27017";

            services.AddTransient<ISettingsRepository>(settingsRepo =>
                new SettingsRepository(connectionString, databaseName));
        }
    }
}
