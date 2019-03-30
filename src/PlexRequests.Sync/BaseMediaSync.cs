using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlexRequests.Plex;
using PlexRequests.Store.Enums;
using PlexRequests.Store.Models;

namespace PlexRequests.Sync
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

        public async Task<SyncResult> SyncMedia(PlexServerLibrary library)
        {
            _logger.LogInformation($"Synchronising Library Type: {library.Type}");

            var result = new SyncResult();

            var libraryContainer = await _plexApi.GetLibrary(AuthToken, PlexUri, library.Key);
            var previouslySyncdMedia = await _plexService.GetMediaItems(MediaType);

            foreach (var remoteMediaItem in libraryContainer.MediaContainer.Metadata)
            {
                var ratingKey = Convert.ToInt32(remoteMediaItem.RatingKey);

                var mediaItem = previouslySyncdMedia.FirstOrDefault(x => x.Key == ratingKey);

                if (mediaItem == null)
                {
                    mediaItem = new PlexMediaItem();
                    result.NewItems.Add(mediaItem);
                }
                else
                {
                    result.ExistingItems.Add(mediaItem);
                }

                mediaItem.Key = ratingKey;
                mediaItem.MediaType = MediaType;
                mediaItem.Title = remoteMediaItem.Title;
                mediaItem.Year = remoteMediaItem.Year;

                var metadataContainer = await _plexApi.GetMetadata(AuthToken, PlexUri, mediaItem.Key);
                var metadata = metadataContainer.MediaContainer.Metadata.FirstOrDefault();

                mediaItem.Resolution = metadata?.Media?.FirstOrDefault()?.VideoResolution;
                SetAgentDetails(mediaItem, metadata?.Guid);

                await GetChildMetadata(mediaItem);
            }

            return result;
        }

        protected void SetAgentDetails(BasePlexMediaItem mediaItem, string agentGuid)
        {
            if (string.IsNullOrEmpty(agentGuid))
            {
                return;
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

            mediaItem.AgentType = matchingSource;
            mediaItem.AgentSourceId = idRaw;
        }

        protected virtual Task GetChildMetadata(PlexMediaItem mediaItem)
        {
            return Task.CompletedTask;
        }
    }
}
