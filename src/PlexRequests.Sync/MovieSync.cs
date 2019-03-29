using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlexRequests.Plex;
using PlexRequests.Store.Models;

namespace PlexRequests.Sync
{
    public class MovieSync : IMediaSync
    {
        private readonly IPlexApi _plexApi;
        private readonly ILogger _logger;

        public MovieSync(
            IPlexApi plexApi,
            ILogger logger
            )
        {
            _plexApi = plexApi;
            _logger = logger;
        }

        public async Task<List<PlexMediaItem>> SyncMedia(PlexServerLibrary library)
        {
            _logger.LogInformation("Synchronising Movies");

            return new List<PlexMediaItem>();
        }
    }
}
