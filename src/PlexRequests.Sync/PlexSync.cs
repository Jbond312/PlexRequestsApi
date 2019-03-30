using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlexRequests.Plex;
using PlexRequests.Settings;

namespace PlexRequests.Sync
{
    public class PlexSync : IPlexSync
    {
        private readonly IPlexApi _plexApi;
        private readonly IPlexService _plexService;
        private readonly PlexSettings _plexSettings;
        private readonly ILogger<PlexSync> _logger;

        public PlexSync(
            IPlexApi plexApi,
            IPlexService plexService,
            IOptions<PlexSettings> plexSettings,
            ILogger<PlexSync> logger
            )
        {
            _plexApi = plexApi;
            _plexService = plexService;
            _plexSettings = plexSettings.Value;
            _logger = logger;
        }

        public async Task Synchronise()
        {
            var plexServer = await _plexService.GetServer();

            var plexUrl = plexServer.GetPlexUri(_plexSettings.ConnectLocally);

            var librariesToSync = plexServer.Libraries.Where(x => x.IsEnabled).ToList();

            if (!librariesToSync.Any())
            {
                _logger.LogDebug("No Plex libraries have been enabled for synchronisation");
                return;
            }

            var plexLibraryContainer = await _plexApi.GetLibraries(plexServer.AccessToken,
                plexServer.GetPlexUri(_plexSettings.ConnectLocally));



            foreach (var libraryToSync in librariesToSync)
            {
                var existsAsRemoteLibrary = plexLibraryContainer.MediaContainer.Directory.Any(x => x.Key == libraryToSync.Key);

                if (!existsAsRemoteLibrary)
                {
                    _logger.LogInformation("Attempted to sync a local library but it no longer exists remotely");
                    continue;
                }

                IMediaSync syncProcessor = null;

                switch (libraryToSync.Type)
                {
                    case "movie":
                        syncProcessor = new MovieSync(_plexApi, _plexService, _logger);
                        break;
                    case "show":
                        syncProcessor = new TvSync(_plexApi, _plexService, _logger);
                        break;
                    default:
                        throw new ArgumentException($"Unknown Plex library type to synchronise: {libraryToSync.Type}");

                }

                var syncResult = await syncProcessor.SyncMedia(libraryToSync, plexUrl, plexServer.AccessToken);

                await _plexService.CreateMany(syncResult.NewItems);
                await _plexService.UpdateMany(syncResult.ExistingItems);
            }

        }
    }
}
