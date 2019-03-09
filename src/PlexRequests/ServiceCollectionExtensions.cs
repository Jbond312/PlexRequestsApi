using Microsoft.Extensions.DependencyInjection;
using PlexRequests.Plex;

namespace PlexRequests
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterPlexApiRequestsDependencies(this IServiceCollection services)
        {
            services.AddTransient<IPlexApi, PlexApi>();
        }
    }
}
