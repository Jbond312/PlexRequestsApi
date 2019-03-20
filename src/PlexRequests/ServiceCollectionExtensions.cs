using Microsoft.Extensions.DependencyInjection;
using PlexRequests.Api;
using PlexRequests.Plex;

namespace PlexRequests
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterDependencies(this IServiceCollection services)
        {
            services.AddTransient<IPlexApi, PlexApi>();
            services.AddSingleton<IApiService, ApiService>();
            services.AddSingleton<IPlexRequestsHttpClient, PlexRequestsHttpClient>();
        }
    }
}
