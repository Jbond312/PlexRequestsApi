using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Settings;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.Sync.SyncProcessors
{
    public class TvProcessor : ISyncProcessor
    {
        private readonly IPlexApi _plexApi;
        private readonly IPlexService _plexService;
        private readonly IMediaItemProcessor _mediaItemProcessor;
        private readonly PlexSettings _plexSettings;
        private readonly IAgentGuidParser _agentGuidParser;
        private readonly ILogger<TvProcessor> _logger;

        public TvProcessor(
            IPlexApi plexApi,
            IPlexService plexService,
            IMediaItemProcessor mediaItemProcessor,
            PlexSettings plexSettings,
            IAgentGuidParser agentGuidParser,
            ILoggerFactory loggerFactory)
        {
            _plexApi = plexApi;
            _plexService = plexService;
            _mediaItemProcessor = mediaItemProcessor;
            _plexSettings = plexSettings;
            _agentGuidParser = agentGuidParser;
            _logger = loggerFactory.CreateLogger<TvProcessor>();
        }

        public PlexMediaTypes Type => PlexMediaTypes.Show;

        public async Task<SyncResult> Synchronise(PlexMediaContainer libraryContainer, bool fullRefresh, string authToken, string plexUri, string machineIdentifier)
        {
            _logger.LogDebug("Started synchronising Movies");

            var syncResult = new SyncResult();

            var localMediaItems = await _plexService.GetMediaItems(Type);
            var localMediaItemsCount = localMediaItems?.Count ?? 0;
            _logger.LogDebug($"Retrieved '{localMediaItemsCount}' existing media items");

            var processedShowKeys = new List<int>();
            foreach (var remoteMediaItem in libraryContainer.MediaContainer.Metadata)
            {
                var ratingKey = GetRatingKey(remoteMediaItem, fullRefresh);

                _logger.LogDebug($"Processing rating key '{remoteMediaItem.RatingKey}'");

                if (processedShowKeys.Contains(ratingKey))
                {
                    continue;
                }

                processedShowKeys.Add(ratingKey);

                var (isNewItem, mediaItem) =
                    await _mediaItemProcessor.GetMediaItem(ratingKey, Type, localMediaItems, authToken, plexUri, machineIdentifier, _plexSettings.PlexMediaItemUriFormat);

                await GetChildMetadata(mediaItem, authToken, plexUri, machineIdentifier);

                _mediaItemProcessor.UpdateResult(syncResult, isNewItem, mediaItem);
            }

            return syncResult;
        }

        private async Task GetChildMetadata(PlexMediaItem mediaItem, string authToken, string plexUri, string machineIdentifier)
        {
            _logger.LogDebug($"Retrieving all seasons for show '{mediaItem.Title}'");
            var seasonShowItems = await GetChildShowItems(mediaItem.Key, authToken, plexUri);

            foreach (var seasonShowItem in seasonShowItems)
            {
                var plexSeason = new PlexSeason
                {
                    Key = seasonShowItem.RatingKey,
                    Title = seasonShowItem.Title,
                    AgentSourceId = seasonShowItem.AgentSourceId,
                    AgentType = seasonShowItem.AgentType,
                    Season = seasonShowItem.Index,
                    PlexMediaUri = PlexHelper.GenerateMediaItemUri(_plexSettings.PlexMediaItemUriFormat, machineIdentifier, seasonShowItem.RatingKey)
                };

                _logger.LogDebug($"Retrieving all episodes for show '{mediaItem.Title}' season '{plexSeason.Season}'");
                var episodeShowItems = await GetChildShowItems(seasonShowItem.RatingKey, authToken, plexUri);

                plexSeason.Episodes = episodeShowItems.Select(ep => new PlexEpisode
                {
                    Key = ep.RatingKey,
                    Title = ep.Title,
                    AgentSourceId = ep.AgentSourceId,
                    AgentType = ep.AgentType,
                    Episode = ep.Index,
                    Year = ep.Year,
                    Resolution = ep.Resolution,
                    PlexMediaUri = PlexHelper.GenerateMediaItemUri(_plexSettings.PlexMediaItemUriFormat, machineIdentifier, ep.RatingKey)
                }).ToList();

                mediaItem.Seasons.Add(plexSeason);
            }
        }

        private async Task<List<ShowMediaItem>> GetChildShowItems(int parentKey, string authToken, string plexUri)
        {
            var showItems = new List<ShowMediaItem>();
            var childContainer = await _plexApi.GetChildrenMetadata(authToken, plexUri, parentKey);

            if (childContainer?.MediaContainer.Metadata == null)
            {
                return new List<ShowMediaItem>();
            }

            foreach (var child in childContainer.MediaContainer.Metadata)
            {
                _logger.LogDebug($"Adding child item with rating key '{child.RatingKey}'");
                var childKey = Convert.ToInt32(child.RatingKey);
                showItems.Add(await CreateShowItem(childKey, authToken, plexUri));
            }

            return showItems;
        }

        private async Task<ShowMediaItem> CreateShowItem(int key, string authToken, string plexUri)
        {
            var itemInfo = await _plexApi.GetMetadata(authToken, plexUri, key);

            var metadata = itemInfo?.MediaContainer?.Metadata?.FirstOrDefault();

            _logger.LogDebug($"Getting agent details for agent guid '{metadata?.Guid}'");
            var agentDetails = _agentGuidParser.TryGetAgentDetails(metadata?.Guid);

            return new ShowMediaItem
            {
                RatingKey = key,
                Title = metadata?.Title,
                Index = metadata?.Index ?? 0,
                Year = metadata?.Year,
                Resolution = metadata?.Media?.FirstOrDefault()?.VideoResolution,
                AgentType = agentDetails.agentType,
                AgentSourceId = agentDetails.agentSourceId
            };
        }

        private static int GetRatingKey(Metadata remoteMediaItem, bool fullRefresh)
        {
            var rawRatingKey = remoteMediaItem.RatingKey;
            if (!fullRefresh)
            {
                rawRatingKey = remoteMediaItem.GrandParentRatingKey;
            }

            var ratingKey = Convert.ToInt32(rawRatingKey);
            return ratingKey;
        }

        private class ShowMediaItem
        {
            public int RatingKey { get; set; }
            public string Title { get; set; }
            public int Index { get; set; }
            public int? Year { get; set; }
            public string Resolution { get; set; }
            public AgentTypes AgentType { get; set; }
            public string AgentSourceId { get; set; }
        }
    }
}
