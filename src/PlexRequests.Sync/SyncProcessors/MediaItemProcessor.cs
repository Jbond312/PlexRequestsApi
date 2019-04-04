using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PlexRequests.Helpers;
using PlexRequests.Plex;
using PlexRequests.Store.Enums;
using PlexRequests.Store.Models;

namespace PlexRequests.Sync.SyncProcessors
{
    public class MediaItemProcessor : IMediaItemProcessor
    {
        private readonly IPlexApi _plexApi;
        private readonly List<AgentTypes?> _agentTypes;
        private readonly Regex _plexAgentGuidRegex;

        public MediaItemProcessor(
            IPlexApi plexApi
            )
        {
            _plexApi = plexApi;
            _agentTypes = Enum.GetValues(typeof(AgentTypes)).Cast<AgentTypes?>().ToList();
            _plexAgentGuidRegex = new Regex("com\\.plexapp\\.agents\\.(?'agent'.*)://(?'agentId'.*)\\?");
        }

        public async Task<(bool, PlexMediaItem)> GetMediaItem(int ratingKey, PlexMediaTypes mediaType, List<PlexMediaItem> localMedia, string authToken, string plexUri)
        {
            var metadataContainer = await _plexApi.GetMetadata(authToken, plexUri, ratingKey);
            var metadata = metadataContainer.MediaContainer.Metadata?.FirstOrDefault();

            if (metadata == null)
            {
                throw new PlexRequestException("PlexMediaContainer", $"No metadata was found for container with key: {ratingKey}");
            }

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

            var (agentType, agentSourceId) = GetAgentDetails(metadata.Guid);
            mediaItem.AgentType = agentType;
            mediaItem.AgentSourceId = agentSourceId;

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

        public (AgentTypes agentType, string agentSourceId) GetAgentDetails(string agentGuid)
        {
            if (string.IsNullOrEmpty(agentGuid))
            {
                throw new PlexRequestException("PlexMetadataGuid", "The PlexMetadataGuid should not be null or empty", HttpStatusCode.InternalServerError);
            }

            var match = _plexAgentGuidRegex.Match(agentGuid);

            if (!match.Success)
            {
                throw new PlexRequestException("PlexMetadataGuid", "The PlexMetadataGuid was not in the expected format", HttpStatusCode.InternalServerError, agentGuid);
            }

            var agent = match.Groups["agent"].Value;
            var agentId = match.Groups["agentId"].Value;

            var matchingSource = _agentTypes.FirstOrDefault(x =>
                x.ToString().Contains(agent, StringComparison.CurrentCultureIgnoreCase));

            if (matchingSource == null)
            {
                throw new PlexRequestException("PlexMetadataGuid", "No AgentType could be extracted from the agent guid",  HttpStatusCode.InternalServerError, agentGuid);
            }

            if (matchingSource.Value == AgentTypes.TheTvDb && agentId.Contains("/"))
            {
                agentId = agentId.Split("/")[0];
            }

            return (matchingSource.Value, agentId);
        }
    }
}
