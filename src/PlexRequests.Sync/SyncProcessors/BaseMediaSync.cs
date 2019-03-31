using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlexRequests.Helpers;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using PlexRequests.Store.Enums;
using PlexRequests.Store.Models;

namespace PlexRequests.Sync.SyncProcessors
{
    public abstract class BaseMediaSync : IMediaSync
    {
        private readonly IPlexApi _plexApi;
        private readonly IPlexService _plexService;
        private readonly ILogger _logger;

        private readonly List<AgentTypes> _agentTypes;

        protected abstract PlexMediaTypes MediaType { get; }
        protected string AuthToken { get; }
        protected string PlexUri { get; }

        protected BaseMediaSync(
            IPlexApi plexApi,
            IPlexService plexService,
            ILogger logger,
            string authToken,
            string plexUri
        )
        {
            _plexApi = plexApi;
            _plexService = plexService;
            _logger = logger;

            _agentTypes = Enum.GetValues(typeof(AgentTypes)).Cast<AgentTypes>().ToList();
            AuthToken = authToken;
            PlexUri = plexUri;
        }

        public async Task<SyncResult> SyncMedia(PlexServerLibrary library, bool fullRefresh)
        {
            _logger.LogInformation($"Sync processing library type: {library.Type}|{library.Key}");

            var result = new SyncResult();

            var libraryContainer = await GetPlexMediaContainer(library, fullRefresh);

            var localMediaItems = await _plexService.GetMediaItems(MediaType);

            var keysProcessed = new List<int>();
            foreach (var remoteMediaItem in libraryContainer.MediaContainer.Metadata)
            {
                var ratingKey = GetRatingKey(remoteMediaItem, fullRefresh);

                if (keysProcessed.Contains(ratingKey))
                {
                    continue;
                }

                keysProcessed.Add(ratingKey);
                
                var metadataContainer = await _plexApi.GetMetadata(AuthToken, PlexUri, ratingKey);
                var metadata = metadataContainer.MediaContainer.Metadata.FirstOrDefault();
                
                var mediaItem = localMediaItems.FirstOrDefault(x => x.Key == ratingKey);
                if (mediaItem == null)
                {
                    mediaItem = new PlexMediaItem
                    {
                        Key = ratingKey,
                        MediaType = MediaType,
                        Title = metadata?.Title,
                        Year = metadata?.Year ?? 0
                    };
                    result.NewItems.Add(mediaItem);
                }
                else
                {
                    result.ExistingItems.Add(mediaItem);
                }
                
                mediaItem.Resolution = metadata?.Media?.FirstOrDefault()?.VideoResolution;

                var (agentType, agentSourceId) = GetAgentDetails(metadata?.Guid);
                mediaItem.AgentType = agentType;
                mediaItem.AgentSourceId = agentSourceId;

                await GetChildMetadata(mediaItem);
            }

            _logger.LogInformation($"Sync finished processing library type: {library.Type}|{library.Key}");

            return result;
        }

        protected (AgentTypes agentType, string agentSourceId) GetAgentDetails(string agentGuid)
        {
            if (string.IsNullOrEmpty(agentGuid))
            {
                throw new PlexRequestException("PlexMetadataGuid", "The PlexMetadataGuid should not be null or empty", HttpStatusCode.InternalServerError);
            }

            /* The possible strings to split are
             * com.plexapp.agents.thetvdb://73141/15/7?lang=en
             * com.plexapp.agents.imdb://tt1727824?lang=en
             * com.plexapp.agents.themoviedb://446021?lang=en
             */
            const int agentIndex = 3;
            const int identifierIndex = 4;

            var sourceSplit = agentGuid.Split(new[] {'.', ':', '/', '?'}, StringSplitOptions.RemoveEmptyEntries);
            var agentTypeRaw = sourceSplit[agentIndex];
            var idRaw = sourceSplit[identifierIndex];

            var matchingSource = _agentTypes.First(x =>
                x.ToString().Contains(agentTypeRaw, StringComparison.CurrentCultureIgnoreCase));
            return (matchingSource, idRaw);
        }

        protected virtual Task GetChildMetadata(PlexMediaItem mediaItem)
        {
            return Task.CompletedTask;
        }

        private int GetRatingKey(Metadata remoteMediaItem, bool fullRefresh)
        {
            var rawRatingKey = remoteMediaItem.RatingKey;
            if (!fullRefresh && MediaType == PlexMediaTypes.Show)
            {
                rawRatingKey = remoteMediaItem.GrandParentRatingKey;
            }

            var ratingKey = Convert.ToInt32(rawRatingKey);
            return ratingKey;
        }

        private async Task<PlexMediaContainer> GetPlexMediaContainer(PlexServerLibrary library, bool fullRefresh)
        {
            PlexMediaContainer libraryContainer;
            if (fullRefresh)
            {
                libraryContainer = await _plexApi.GetLibrary(AuthToken, PlexUri, library.Key);
            }
            else
            {
                libraryContainer = await _plexApi.GetRecentlyAdded(AuthToken, PlexUri, library.Key);
            }

            return libraryContainer;
        }
    }
}
