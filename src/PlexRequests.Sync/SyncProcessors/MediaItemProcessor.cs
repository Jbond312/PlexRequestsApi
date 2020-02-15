using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlexRequests.Core.Helpers;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;

namespace PlexRequests.Sync.SyncProcessors
{
    public class MediaItemProcessor : IMediaItemProcessor
    {
        private readonly IPlexApi _plexApi;
        private readonly IAgentGuidParser _agentGuidParser;
        private readonly ILogger<MediaItemProcessor> _logger;

        public MediaItemProcessor(
            IPlexApi plexApi,
            IAgentGuidParser agentGuidParser,
            ILogger<MediaItemProcessor> logger
            )
        {
            _plexApi = plexApi;
            _agentGuidParser = agentGuidParser;
            _logger = logger;
        }

        public async Task<MediaItemResult> GetMediaItem(int ratingKey, PlexMediaTypes mediaType, List<PlexMediaItemRow> localMedia, string authToken, string plexUri, string machineIdentifier, string plexUriFormat)
        {
            var metadata = await TryGetPlexMetadata(ratingKey, authToken, plexUri);

            if (metadata == null)
            {
                return null;
            }

            var mediaItem = localMedia.FirstOrDefault(x => x.MediaItemKey == ratingKey);
            var isNewItem = false;
            if (mediaItem == null)
            {
                mediaItem = new PlexMediaItemRow
                {
                    Identifier = Guid.NewGuid(),
                    MediaItemKey = ratingKey,
                    MediaType = mediaType,
                    Title = metadata.Title,
                    Year = metadata.Year
                };
                isNewItem = true;
            }

            mediaItem.Resolution = metadata.Media?.FirstOrDefault()?.VideoResolution;

            var agentResult = _agentGuidParser.TryGetAgentDetails(metadata.Guid);

            if (agentResult == null)
            {
                return null;
            }

            mediaItem.AgentType = agentResult.AgentType;
            mediaItem.AgentSourceId = agentResult.AgentSourceId;

            mediaItem.MediaUri = PlexHelper.GenerateMediaItemUri(plexUriFormat, machineIdentifier, ratingKey);

            return new MediaItemResult(isNewItem, mediaItem);
        }

        public void UpdateResult(SyncResult syncResult, bool isNew, PlexMediaItemRow mediaItem)
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
                _logger.LogError($"No metadata was found for the container with key: {ratingKey}");
                return null;
            }

            return metadata;
        }
    }
}
