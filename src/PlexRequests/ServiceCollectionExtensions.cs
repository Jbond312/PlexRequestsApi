using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using PlexRequests.Api;
using PlexRequests.ApiRequests.Search.Helpers;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Services;
using PlexRequests.Core.Settings;
using PlexRequests.DataAccess;
using PlexRequests.Plex;
using PlexRequests.Plex.MediaItemRetriever;
using PlexRequests.Sync;
using PlexRequests.Sync.SyncProcessors;
using PlexRequests.TheMovieDb;

namespace PlexRequests
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterDependencies(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            RegisterServices(services);
            RegisterDbContext(services, configuration, environment);
        }

        private static void RegisterDbContext(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.AddTransient<DataAccess.Repositories.IUserRepository, DataAccess.Repositories.UserRepository>();
            services.AddTransient<DataAccess.Repositories.IPlexServerRepository, DataAccess.Repositories.PlexServerRepository>();
            services.AddTransient<DataAccess.Repositories.IPlexMediaItemRepository, DataAccess.Repositories.PlexMediaItemRepository>();
            services.AddTransient<DataAccess.Repositories.IMovieRequestRepository, DataAccess.Repositories.MovieRequestRepository>();
            services.AddTransient<DataAccess.Repositories.ITvRequestRepository, DataAccess.Repositories.TvRequestRepository>();
            services.AddTransient<DataAccess.Repositories.IIssuesRepository, DataAccess.Repositories.IssuesRepository>();

            var connectionString = environment.IsProduction() ? configuration["PlexRequestsDataContext"] : configuration.GetConnectionString("PlexRequestsDataContext");

            services.AddDbContext<PlexRequestsDataContext>(
                options =>
                {
                    options.UseSqlServer(connectionString);
                });
            services.AddTransient<IUnitOfWork, UnitOfWork>();
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
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IPlexService, PlexService>();
            services.AddTransient<IMovieRequestService, MovieRequestService>();
            services.AddTransient<ITvRequestService, TvRequestService>();
            services.AddTransient<ICompletionService, CompletionService>();
            services.AddTransient<IIssueService, IssueService>();
            services.AddTransient<ITheMovieDbService, TheMovieDbService>();
            services.AddTransient<IPlexSync, PlexSync>();
            services.AddTransient<IMediaItemRetriever, MovieMediaItemRetriever>();
            services.AddTransient<IMediaItemRetriever, TvMediaItemRetriever>();

            services.AddTransient<ICacheService, CacheService>();
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<ITokenService, TokenService>();
            services.AddTransient<IApiService, ApiService>();
            services.AddTransient<IMediaItemProcessor, MediaItemProcessor>();
            services.AddTransient<IProcessorProvider, ProcessorProvider>();
            services.AddTransient<IPlexRequestsHttpClient, PlexRequestsHttpClient>();
            services.AddTransient<IAgentGuidParser, AgentGuidParser>();
            services.AddTransient<IRequestHelper, RequestHelper>();
            services.AddTransient<IMovieQueryHelper, MovieQueryHelper>();
            services.AddTransient<ITvQueryHelper, TvQueryHelper>();
        }

        private static async Task OnTokenValidated(TokenValidatedContext tokenValidatedContext)
        {
            var userService = tokenValidatedContext.HttpContext.RequestServices.GetService<IUserService>();
            var userIdClaim = tokenValidatedContext.Principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim?.Value))
            {
                return;
            }

            var user = await userService.GetUser(int.Parse(userIdClaim.Value));

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
