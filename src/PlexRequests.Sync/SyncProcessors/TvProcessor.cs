﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Settings;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;

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

                var retrievedItem = await _mediaItemProcessor.GetMediaItem(ratingKey, Type, localMediaItems, authToken, plexUri, machineIdentifier, _plexSettings.PlexMediaItemUriFormat);

                if (retrievedItem == null)
                {
                    _logger.LogError($"Skipping rating key '{ratingKey}' as no metadata could be retrieved.");
                    _logger.LogDebug($"Finished processing rating key '{remoteMediaItem.RatingKey}'");
                    continue;
                }

                await GetChildMetadata(retrievedItem.MediaItem, authToken, plexUri, machineIdentifier);

                _mediaItemProcessor.UpdateResult(syncResult, retrievedItem.IsNew, retrievedItem.MediaItem);

                _logger.LogDebug($"Finished processing rating key '{remoteMediaItem.RatingKey}'");
            }

            return syncResult;
        }

        private async Task GetChildMetadata(PlexMediaItemRow mediaItem, string authToken, string plexUri, string machineIdentifier)
        {
            _logger.LogDebug($"Retrieving all seasons for show '{mediaItem.Title}'");
            var seasonShowItems = await GetChildShowItems(mediaItem.MediaItemKey, authToken, plexUri);

            foreach (var seasonShowItem in seasonShowItems)
            {
                var plexSeason = mediaItem.PlexSeasons.FirstOrDefault(x => x.Season == seasonShowItem.Index);

                if (plexSeason == null)
                {
                    plexSeason = new PlexSeasonRow
                    {
                        Identifier = Guid.NewGuid()
                    };

                    mediaItem.PlexSeasons.Add(plexSeason);
                }

                plexSeason.MediaItemKey = seasonShowItem.RatingKey;
                plexSeason.Title = seasonShowItem.Title;
                plexSeason.AgentSourceId = seasonShowItem.AgentSourceId;
                plexSeason.AgentType = seasonShowItem.AgentType;
                plexSeason.Season = seasonShowItem.Index;
                plexSeason.MediaUri = PlexHelper.GenerateMediaItemUri(_plexSettings.PlexMediaItemUriFormat, machineIdentifier, seasonShowItem.RatingKey);

                _logger.LogDebug($"Retrieving all episodes for show '{mediaItem.Title}' season '{plexSeason.Season}'");
                var episodeShowItems = await GetChildShowItems(seasonShowItem.RatingKey, authToken, plexUri);

                foreach (var episodeShowItem in episodeShowItems)
                {
                    var plexEpisode = plexSeason.PlexEpisodes.FirstOrDefault(x => x.Episode == episodeShowItem.Index);

                    if (plexEpisode == null)
                    {
                        plexEpisode = new PlexEpisodeRow
                        {
                            Identifier = Guid.NewGuid()
                        };

                        plexSeason.PlexEpisodes.Add(plexEpisode);
                    }

                    plexEpisode.Identifier = Guid.NewGuid();
                    plexEpisode.MediaItemKey = episodeShowItem.RatingKey;
                    plexEpisode.Title = episodeShowItem.Title;
                    plexEpisode.AgentSourceId = episodeShowItem.AgentSourceId;
                    plexEpisode.AgentType = episodeShowItem.AgentType;
                    plexEpisode.Episode = episodeShowItem.Index;
                    plexEpisode.Year = episodeShowItem.Year;
                    plexEpisode.Resolution = episodeShowItem.Resolution;
                    plexEpisode.MediaUri = PlexHelper.GenerateMediaItemUri(_plexSettings.PlexMediaItemUriFormat, machineIdentifier, episodeShowItem.RatingKey);
                }
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

                var createdShowItem = await CreateShowItem(childKey, authToken, plexUri);
                if (createdShowItem != null)
                {
                    showItems.Add(createdShowItem);
                }
            }

            return showItems;
        }

        private async Task<ShowMediaItem> CreateShowItem(int key, string authToken, string plexUri)
        {
            var itemInfo = await _plexApi.GetMetadata(authToken, plexUri, key);

            var metadata = itemInfo?.MediaContainer?.Metadata?.FirstOrDefault();

            _logger.LogDebug($"Getting agent details for agent guid '{metadata?.Guid}'");
            var agentDetails = _agentGuidParser.TryGetAgentDetails(metadata?.Guid);

            if (agentDetails == null)
            {
                return null;
            }

            return new ShowMediaItem
            {
                RatingKey = key,
                Title = metadata?.Title,
                Index = metadata?.Index ?? 0,
                Year = metadata?.Year,
                Resolution = metadata?.Media?.FirstOrDefault()?.VideoResolution,
                AgentType = agentDetails.AgentType,
                AgentSourceId = agentDetails.AgentSourceId
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
