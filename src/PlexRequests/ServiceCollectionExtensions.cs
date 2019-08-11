using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PlexRequests.Api;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Services;
using PlexRequests.Core.Settings;
using PlexRequests.Plex;
using PlexRequests.Repository;
using PlexRequests.Sync;
using PlexRequests.Sync.SyncProcessors;
using PlexRequests.TheMovieDb;

namespace PlexRequests
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterDependencies(this IServiceCollection services, DatabaseSettings databaseSettings)
        {
            RegisterServices(services);
            RegisterRepositories(services, databaseSettings);
        }

        public static void ConfigureJwtAuthentication(this IServiceCollection services, AuthenticationSettings authSettings, bool isProduction)
        {
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.Events = new JwtBearerEvents
                {
                    OnTokenValidated = OnTokenValidated
                };
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
            services.AddTransient<IClaimsPrincipalAccessor, ClaimsPrincipalAccessor>();
            services.AddTransient<IPlexApi, PlexApi>();
            services.AddTransient<ITheMovieDbApi, TheMovieDbApi>();
            services.AddTransient<ICacheService, CacheService>();
            services.AddTransient<ISettingsService, SettingsService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IPlexService, PlexService>();
            services.AddTransient<IRequestService, RequestService>();
            services.AddTransient<ICompletionService, CompletionService>();
            services.AddTransient<IIssueService, IssueService>();
            services.AddTransient<IPlexSync, PlexSync>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ITokenService, TokenService>();
            services.AddSingleton<IApiService, ApiService>();
            services.AddSingleton<IMediaItemProcessor, MediaItemProcessor>();
            services.AddSingleton<IProcessorProvider, ProcessorProvider>();
            services.AddSingleton<IPlexRequestsHttpClient, PlexRequestsHttpClient>();
            services.AddSingleton<IAgentGuidParser, AgentGuidParser>();
            services.AddSingleton<IRequestHelper, RequestHelper>();
        }

        private static void RegisterRepositories(IServiceCollection services, DatabaseSettings databaseSettings)
        {
            var connectionString = $"mongodb://{databaseSettings.User}:{databaseSettings.Password}@{databaseSettings.Server}:27017?connectTimeoutMS=30000";

            services.AddTransient<ISettingsRepository>(repo =>
                new SettingsRepository(connectionString, databaseSettings.Database));
            services.AddTransient<IUserRepository>(repo =>
                new UserRepository(connectionString, databaseSettings.Database));
            services.AddTransient<IPlexServerRepository>(repo =>
                new PlexServerRepository(connectionString, databaseSettings.Database));
            services.AddTransient<IPlexMediaRepository>(repo =>
                new PlexMediaRepository(connectionString, databaseSettings.Database));
            services.AddTransient<IRequestRepository>(repo =>
                new RequestRepository(connectionString, databaseSettings.Database));
            services.AddTransient<IIssuesRepository>(repo =>
                new IssuesRepository(connectionString, databaseSettings.Database));
        }

        private static async Task OnTokenValidated(TokenValidatedContext tokenValidatedContext)
        {
            var userService = tokenValidatedContext.HttpContext.RequestServices.GetService<IUserService>();
            var userIdClaim = tokenValidatedContext.Principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim?.Value))
            {
                return;
            }

            var user = await userService.GetUser(Guid.Parse(userIdClaim.Value));

            if (user == null)
            {
                tokenValidatedContext.Fail("User does not exist.");
            }
            else if (user.IsDisabled)
            {
                tokenValidatedContext.Fail("User has been disabled.");
            }
        }
    }
}
