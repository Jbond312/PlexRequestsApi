using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PlexRequests.Api;
using PlexRequests.Core;
using PlexRequests.Helpers;
using PlexRequests.Plex;
using PlexRequests.Settings;
using PlexRequests.Store;
using PlexRequests.TheMovieDb;

namespace PlexRequests
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterDependencies(this IServiceCollection services, Store.Models.Settings settings)
        {
            RegisterServices(services);
            RegisterRepositories(services, settings);
        }

        public static void ConfigureJwtAuthentication(this IServiceCollection services, AuthenticationSettings authSettings, bool isProduction)
        {
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = isProduction;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(authSettings.SecretKey)),
                    RequireExpirationTime = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = "PlexRequests",
                    ValidAudience = "PlexRequests",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddTransient<IPlexApi, PlexApi>();
            services.AddTransient<ITheMovieDbApi, TheMovieDbApi>();
            services.AddSingleton<IApiService, ApiService>();
            services.AddTransient<ICacheService, CacheService>();
            services.AddTransient<ISettingsService, SettingsService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IPlexService, PlexService>();
            services.AddSingleton<IPlexRequestsHttpClient, PlexRequestsHttpClient>();
        }

        private static void RegisterRepositories(IServiceCollection services, Store.Models.Settings settings)
        {
            var connectionString = $"mongodb://{settings.DatabaseServer}:27017?connectTimeoutMS=30000";

            services.AddTransient<ISettingsRepository>(repo =>
                new SettingsRepository(connectionString, settings.DatabaseName));
            services.AddTransient<IUserRepository>(repo =>
                new UserRepository(connectionString, settings.DatabaseName));
            services.AddTransient<IPlexServerRepository>(repo =>
                new PlexServerRepository(connectionString, settings.DatabaseName));
        }
    }
}
