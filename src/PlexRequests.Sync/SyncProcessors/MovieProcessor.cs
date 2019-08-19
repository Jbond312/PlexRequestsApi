using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlexRequests.Core.Settings;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using PlexRequests.Repository.Enums;

namespace PlexRequests.Sync.SyncProcessors
{
    public class MovieProcessor : ISyncProcessor
    {
        private readonly IPlexService _plexService;
        private readonly IMediaItemProcessor _mediaItemProcessor;
        private readonly PlexSettings _plexSettings;
        private readonly ILogger<MovieProcessor> _logger;

        public MovieProcessor(
            IPlexService plexService,
            IMediaItemProcessor mediaItemProcessor,
            PlexSettings plexSettings,
            ILoggerFactory loggerFactory)
        {
            _plexService = plexService;
            _mediaItemProcessor = mediaItemProcessor;
            _plexSettings = plexSettings;
            _logger = loggerFactory.CreateLogger<MovieProcessor>();
        }

        public PlexMediaTypes Type => PlexMediaTypes.Movie;

        public async Task<SyncResult> Synchronise(PlexMediaContainer libraryContainer, bool fullRefresh, string authToken, string plexUri, string machineIdentifier)
        {
            _logger.LogDebug("Started synchronising Movies");

            var syncResult = new SyncResult();

            var localMediaItems = await _plexService.GetMediaItems(Type);
            var localMediaItemsCount = localMediaItems?.Count ?? 0;
            _logger.LogDebug($"Retrieved '{localMediaItemsCount}' existing media items");

            foreach (var remoteMediaItem in libraryContainer.MediaContainer.Metadata)
            {
                _logger.LogDebug($"Processing rating key '{remoteMediaItem.RatingKey}'");

                var ratingKey = Convert.ToInt32(remoteMediaItem.RatingKey);

                var retrievedItem = await _mediaItemProcessor.GetMediaItem(ratingKey, Type, localMediaItems, authToken, plexUri, machineIdentifier, _plexSettings.PlexMediaItemUriFormat);

                _logger.LogDebug($"Finished processing rating key '{remoteMediaItem.RatingKey}'");

                if (retrievedItem == null)
                {
                    continue;
                }

                _mediaItemProcessor.UpdateResult(syncResult, retrievedItem.IsNew, retrievedItem.MediaItem);
            }

            _logger.LogDebug("Finished synchronising Movies");

            return syncResult;
        }
    }
}
