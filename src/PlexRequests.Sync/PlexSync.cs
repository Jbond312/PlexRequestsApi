using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlexRequests.Plex;
using PlexRequests.Settings;
using PlexRequests.Store.Models;
using PlexRequests.Sync.SyncProcessors;

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

        public async Task Synchronise(bool fullRefresh)
        {
            var plexServer = await _plexService.GetServer();

            var plexUrl = plexServer.GetPlexUri(_plexSettings.ConnectLocally);

            var librariesToSync = plexServer.Libraries.Where(x => x.IsEnabled).ToList();

            if (!librariesToSync.Any())
            {
                _logger.LogDebug("No Plex libraries have been enabled for synchronisation");
                return;
            }

            if (fullRefresh)
            {
                await _plexService.DeleteAllMediaItems();
            }

            var plexLibraryContainer = await _plexApi.GetLibraries(plexServer.AccessToken,
                plexServer.GetPlexUri(_plexSettings.ConnectLocally));

            foreach (var libraryToSync in librariesToSync)
            {
                var existsAsRemoteLibrary =
                    plexLibraryContainer.MediaContainer.Directory.Any(x => x.Key == libraryToSync.Key);

                if (!existsAsRemoteLibrary)
                {
                    _logger.LogInformation($"Attempted to sync the local library '{libraryToSync.Type}|{libraryToSync.Key}' but it no longer exists remotely");
                    continue;
                }

                var syncProcessor = GetSyncProcessor(libraryToSync, plexServer, plexUrl);

                if (syncProcessor == null)
                {
                    return;
                }

                var syncResult = await syncProcessor.SyncMedia(libraryToSync, fullRefresh);

                _logger.LogInformation($"Sync Results. Create: {syncResult.NewItems.Count} Update: {syncResult.ExistingItems.Count}");

                await _plexService.CreateMany(syncResult.NewItems);
                await _plexService.UpdateMany(syncResult.ExistingItems);
            }

        }

        private IMediaSync GetSyncProcessor(PlexServerLibrary libraryToSync, PlexServer plexServer, string plexUrl)
        {
            IMediaSync syncProcessor = null;

            switch (libraryToSync.Type)
            {
                case "movie":
                    syncProcessor = new MovieSync(_plexApi, _plexService, _logger, plexServer.AccessToken, plexUrl);
                    break;
                case "show":
                    syncProcessor = new TvSync(_plexApi, _plexService, _logger, plexServer.AccessToken, plexUrl);
                    break;
                default:
                    _logger.LogInformation($"Unable to sync Plex library type '{libraryToSync.Type}' as it is not supported.");
                    break;
            }

            return syncProcessor;
        }
    }
}
