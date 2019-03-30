using Microsoft.Extensions.Logging;
using PlexRequests.Plex;
using PlexRequests.Store.Enums;

namespace PlexRequests.Sync
{
    public class MovieSync : BaseMediaSync
    {
        public MovieSync(
            IPlexApi plexApi,
            IPlexService plexService,
            ILogger logger,
            string authToken,
            string plexUri
            ) : base(plexApi, plexService, logger, authToken, plexUri)
        {
        }

        protected override PlexMediaTypes MediaType => PlexMediaTypes.Movie;
    }
}
