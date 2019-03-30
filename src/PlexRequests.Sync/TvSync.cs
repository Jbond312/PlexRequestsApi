using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlexRequests.Plex;
using PlexRequests.Store.Models;

namespace PlexRequests.Sync
{
    public class TvSync : IMediaSync
    {
        private readonly IPlexApi _plexApi;
        private readonly IPlexService _plexService;
        private readonly ILogger _logger;

        public TvSync(IPlexApi plexApi,
            IPlexService plexService,
            ILogger logger
            )
        {
            _plexApi = plexApi;
            _plexService = plexService;
            _logger = logger;
        }

        public async Task<SyncResult> SyncMedia(PlexServerLibrary library, string plexUri, string accessToken)
        {
            _logger.LogInformation("Synchronising TVShows");

            return new SyncResult();
        }
    }
}
