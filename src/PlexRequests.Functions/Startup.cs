using System;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlexRequests.Api;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Services;
using PlexRequests.Core.Settings;
using PlexRequests.DataAccess;
using PlexRequests.Functions;
using PlexRequests.Functions.AccessTokens;
using PlexRequests.Functions.Features.Search.Helpers;
using PlexRequests.Plex;
using PlexRequests.Plex.MediaItemRetriever;
using PlexRequests.Sync;
using PlexRequests.Sync.SyncProcessors;
using PlexRequests.TheMovieDb;

[assembly: FunctionsStartup(typeof(Startup))]
namespace PlexRequests.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddMemoryCache();
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            builder.Services.AddAutoMapper(assemblies);
            builder.Services.AddMediatR(assemblies);

            var configuration = new ConfigurationBuilder()
#if DEBUG
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
#endif
                .AddEnvironmentVariables()
                .AddEnvironmentVariables("APPSETTING_")
                .Build();

            builder.Services.AddOptions<AuthenticationSettings>().Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection(nameof(AuthenticationSettings)).Bind(settings);
            });
            builder.Services.AddOptions<TheMovieDbSettings>().Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection(nameof(TheMovieDbSettings)).Bind(settings);
            });
            builder.Services.AddOptions<PlexSettings>().Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection(nameof(PlexSettings)).Bind(settings);
            });
            builder.Services.AddOptions<PlexRequestsSettings>().Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection(nameof(PlexRequestsSettings)).Bind(settings);
            });

            RegisterServices(builder.Services);
            RegisterDbContext(builder.Services, configuration);
        }

        private static void RegisterDbContext(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<DataAccess.Repositories.IUserRepository, DataAccess.Repositories.UserRepository>();
            services.AddTransient<DataAccess.Repositories.IPlexServerRepository, DataAccess.Repositories.PlexServerRepository>();
            services.AddTransient<DataAccess.Repositories.IPlexMediaItemRepository, DataAccess.Repositories.PlexMediaItemRepository>();
            services.AddTransient<DataAccess.Repositories.IMovieRequestRepository, DataAccess.Repositories.MovieRequestRepository>();
            services.AddTransient<DataAccess.Repositories.ITvRequestRepository, DataAccess.Repositories.TvRequestRepository>();
            services.AddTransient<DataAccess.Repositories.IIssuesRepository, DataAccess.Repositories.IssuesRepository>();

            var connectionString = Environment.GetEnvironmentVariable("PlexRequestsDataContext");

            services.AddDbContext<PlexRequestsDataContext>(
                options =>
                {
                    options.UseSqlServer(configuration.GetConnectionString("PlexRequestsDataContext"));
                });
            services.AddTransient<IUnitOfWork, UnitOfWork>();
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IAccessTokenProvider, AccessTokenProvider>();
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
            //services.AddTransient<ITvQueryHelper, TvQueryHelper>();
        }
    }
}