using System;
using System.Threading.Tasks;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using PlexRequests.Store.Enums;

namespace PlexRequests.Sync.SyncProcessors
{
    public class MovieProcessor : ISyncProcessor
    {
        private readonly IPlexService _plexService;
        private readonly IMediaItemProcessor _mediaItemProcessor;

        public MovieProcessor(
            IPlexService plexService,
            IMediaItemProcessor mediaItemProcessor
        )
        {
            _plexService = plexService;
            _mediaItemProcessor = mediaItemProcessor;
        }

        public PlexMediaTypes Type => PlexMediaTypes.Movie;

        public async Task<SyncResult> Synchronise(PlexMediaContainer libraryContainer, bool fullRefresh, string authToken, string plexUri)
        {
            var syncResult = new SyncResult();

            const PlexMediaTypes mediaType = PlexMediaTypes.Movie;

            var localMediaItems = await _plexService.GetMediaItems(mediaType);

            foreach (var remoteMediaItem in libraryContainer.MediaContainer.Metadata)
            {
                var ratingKey = Convert.ToInt32(remoteMediaItem.RatingKey);

                var (isNewItem, mediaItem) =
                    await _mediaItemProcessor.GetMediaItem(ratingKey, mediaType, localMediaItems, authToken, plexUri);
                
                _mediaItemProcessor.UpdateResult(syncResult, isNewItem, mediaItem);
            }

            return syncResult;
        }
    }
}
