using System;
using System.Threading.Tasks;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using PlexRequests.Settings;
using PlexRequests.Store.Enums;

namespace PlexRequests.Sync.SyncProcessors
{
    public class MovieProcessor : ISyncProcessor
    {
        private readonly IPlexService _plexService;
        private readonly IMediaItemProcessor _mediaItemProcessor;
        private readonly PlexSettings _plexSettings;

        public MovieProcessor(
            IPlexService plexService,
            IMediaItemProcessor mediaItemProcessor, 
            PlexSettings plexSettings)
        {
            _plexService = plexService;
            _mediaItemProcessor = mediaItemProcessor;
            _plexSettings = plexSettings;
        }

        public PlexMediaTypes Type => PlexMediaTypes.Movie;

        public async Task<SyncResult> Synchronise(PlexMediaContainer libraryContainer, bool fullRefresh, string authToken, string plexUri, string machineIdentifier)
        {
            var syncResult = new SyncResult();

            var localMediaItems = await _plexService.GetMediaItems(Type);

            foreach (var remoteMediaItem in libraryContainer.MediaContainer.Metadata)
            {
                var ratingKey = Convert.ToInt32(remoteMediaItem.RatingKey);

                var (isNewItem, mediaItem) =
                    await _mediaItemProcessor.GetMediaItem(ratingKey, Type, localMediaItems, authToken, plexUri, machineIdentifier, _plexSettings.PlexMediaItemUriFormat);
                
                _mediaItemProcessor.UpdateResult(syncResult, isNewItem, mediaItem);
            }

            return syncResult;
        }
    }
}
