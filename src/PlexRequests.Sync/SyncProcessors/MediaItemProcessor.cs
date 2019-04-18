using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Helpers;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.Sync.SyncProcessors
{
    public class MediaItemProcessor : IMediaItemProcessor
    {
        private readonly IPlexApi _plexApi;
        private readonly IAgentGuidParser _agentGuidParser;

        public MediaItemProcessor(
            IPlexApi plexApi,
            IAgentGuidParser agentGuidParser
            )
        {
            _plexApi = plexApi;
            _agentGuidParser = agentGuidParser;
        }

        public async Task<(bool, PlexMediaItem)> GetMediaItem(int ratingKey, PlexMediaTypes mediaType, List<PlexMediaItem> localMedia, string authToken, string plexUri, string machineIdentifier, string plexUriFormat)
        {
            var metadata = await TryGetPlexMetadata(ratingKey, authToken, plexUri);

            var mediaItem = localMedia.FirstOrDefault(x => x.Key == ratingKey);
            var isNewItem = false;
            if (mediaItem == null)
            {
                mediaItem = new PlexMediaItem
                {
                    Key = ratingKey,
                    MediaType = mediaType,
                    Title = metadata.Title,
                    Year = metadata.Year
                };
                isNewItem = true;
            }

            mediaItem.Resolution = metadata.Media?.FirstOrDefault()?.VideoResolution;

            var (agentType, agentSourceId) = _agentGuidParser.TryGetAgentDetails(metadata.Guid);
            mediaItem.AgentType = agentType;
            mediaItem.AgentSourceId = agentSourceId;

            mediaItem.PlexMediaUri = PlexHelper.GenerateMediaItemUri(plexUriFormat, machineIdentifier, ratingKey);
            
            return (isNewItem, mediaItem);
        }

        public void UpdateResult(SyncResult syncResult, bool isNew, PlexMediaItem mediaItem)
        {
            if (isNew)
            {
                syncResult.NewItems.Add(mediaItem);
            }
            else
            {
                syncResult.ExistingItems.Add(mediaItem);
            }
        }

        private async Task<Metadata> TryGetPlexMetadata(int ratingKey, string authToken, string plexUri)
        {
            var metadataContainer = await _plexApi.GetMetadata(authToken, plexUri, ratingKey);
            var metadata = metadataContainer.MediaContainer.Metadata?.FirstOrDefault();

            if (metadata == null)
            {
                throw new PlexRequestException("Plex Metadata Error",
                    $"No metadata was found for container with key: {ratingKey}");
            }

            return metadata;
        }
    }
}
