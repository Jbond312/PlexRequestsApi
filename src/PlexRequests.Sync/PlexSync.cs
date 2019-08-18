using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlexRequests.Core.Services;
using PlexRequests.Core.Settings;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;
using PlexRequests.Sync.SyncProcessors;

namespace PlexRequests.Sync
{
    public class PlexSync : IPlexSync
    {
        private readonly IPlexApi _plexApi;
        private readonly IPlexService _plexService;
        private readonly ICompletionService _completionService;
        private readonly IProcessorProvider _processorProvider;
        private readonly PlexSettings _plexSettings;
        private readonly ILogger<PlexSync> _logger;

        public PlexSync(IPlexApi plexApi,
            IPlexService plexService,
            ICompletionService completionService,
            IProcessorProvider processorProvider,
            IOptions<PlexSettings> plexSettings,
            ILogger<PlexSync> logger)
        {
            _plexApi = plexApi;
            _plexService = plexService;
            _completionService = completionService;
            _processorProvider = processorProvider;
            _plexSettings = plexSettings.Value;
            _logger = logger;
        }

        public async Task Synchronise(bool fullRefresh)
        {
            var refreshType = fullRefresh ? "full" : "partial";

            _logger.LogInformation($"Starting a {refreshType} sync with Plex.");

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

            var plexLibraryContainer = await _plexApi.GetLibraries(plexServer.AccessToken, plexUrl);

            foreach (var libraryToSync in librariesToSync)
            {
                var existsAsRemoteLibrary =
                    plexLibraryContainer.MediaContainer.Directory.Any(x => x.Key == libraryToSync.Key);

                if (!existsAsRemoteLibrary)
                {
                    _logger.LogInformation($"Attempted to sync the local library '{libraryToSync.Type}|{libraryToSync.Key}' but it no longer exists remotely");
                    continue;
                }

                var syncProcessor = _processorProvider.GetProcessor(libraryToSync.Type);

                if (syncProcessor == null)
                {
                    _logger.LogInformation($"Attempted to sync the local library '{libraryToSync.Type}|{libraryToSync.Key}' but the type is not supported");
                    return;
                }

                await SynchroniseLibrary(fullRefresh, libraryToSync, plexServer, plexUrl, syncProcessor);
            }
        }

        private async Task SynchroniseLibrary(bool fullRefresh, PlexServerLibrary libraryToSync, PlexServer plexServer,
            string plexUrl, ISyncProcessor syncProcessor)
        {
            _logger.LogInformation($"Sync processing library type: {libraryToSync.Type}|{libraryToSync.Key}");

            var libraryContainer =
                await GetLibraryContainer(libraryToSync, fullRefresh, plexServer.AccessToken, plexUrl);

            var syncResult = await syncProcessor.Synchronise(libraryContainer, fullRefresh, plexServer.AccessToken, plexUrl, plexServer.MachineIdentifier);

            _logger.LogInformation($"Sync finished processing library type: {libraryToSync.Type}|{libraryToSync.Key}");

            _logger.LogInformation($"Sync Results. Create: {syncResult.NewItems.Count} Update: {syncResult.ExistingItems.Count}");

            await _plexService.CreateMany(syncResult.NewItems);
            await _plexService.UpdateMany(syncResult.ExistingItems);
            await AutoCompleteRequests(syncResult, syncProcessor.Type);
        }

        private async Task AutoCompleteRequests(SyncResult syncResult, PlexMediaTypes syncProcessorType)
        {
            var plexKeysByAgentType = syncResult.NewItems.Concat(syncResult.ExistingItems)
                                                .ToDictionary(x => new MediaAgent(x.AgentType, x.AgentSourceId),
                                                    x => x);

            await _completionService.AutoCompleteRequests(plexKeysByAgentType, syncProcessorType);
        }

        private async Task<PlexMediaContainer> GetLibraryContainer(PlexServerLibrary library, bool fullRefresh, string authToken, string plexUri)
        {
            PlexMediaContainer libraryContainer;
            if (fullRefresh)
            {
                libraryContainer = await _plexApi.GetLibrary(authToken, plexUri, library.Key);
            }
            else
            {
                libraryContainer = await _plexApi.GetRecentlyAdded(authToken, plexUri, library.Key);
            }

            return libraryContainer;
        }
    }
}
