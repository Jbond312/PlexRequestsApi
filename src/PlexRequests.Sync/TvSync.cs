using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlexRequests.Plex;
using PlexRequests.Store.Models;

namespace PlexRequests.Sync
{
    public class TvSync : IMediaSync
    {
        private readonly IPlexApi _plexApi;
        private readonly ILogger _logger;

        public TvSync(
            IPlexApi plexApi,
            ILogger logger
        )
        {
            _plexApi = plexApi;
            _logger = logger;
        }

        public async Task<List<PlexMediaItem>> SyncMedia(PlexServerLibrary library)
        {
            _logger.LogInformation("Synchronising TVShows");

            return new List<PlexMediaItem>();
        }
    }
}
