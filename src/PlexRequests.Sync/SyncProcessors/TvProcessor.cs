﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using PlexRequests.Store.Enums;
using PlexRequests.Store.Models;

namespace PlexRequests.Sync.SyncProcessors
{
    public class TvProcessor : ISyncProcessor
    {
        private readonly IPlexApi _plexApi;
        private readonly IPlexService _plexService;
        private readonly IMediaItemProcessor _mediaItemProcessor;

        public TvProcessor(
            IPlexApi plexApi,
            IPlexService plexService,
            IMediaItemProcessor mediaItemProcessor
            )
        {
            _plexApi = plexApi;
            _plexService = plexService;
            _mediaItemProcessor = mediaItemProcessor;
        }

        public PlexMediaTypes Type => PlexMediaTypes.Show;

        public async Task<SyncResult> Synchronise(PlexMediaContainer libraryContainer, bool fullRefresh, string authToken, string plexUri)
        {
            var syncResult = new SyncResult();

            const PlexMediaTypes mediaType = PlexMediaTypes.Show;

            var localMediaItems = await _plexService.GetMediaItems(x => x.MediaType == mediaType);

            var processedShowKeys = new List<int>();
            foreach (var remoteMediaItem in libraryContainer.MediaContainer.Metadata)
            {
                var ratingKey = GetRatingKey(remoteMediaItem, fullRefresh);

                if (processedShowKeys.Contains(ratingKey))
                {
                    continue;
                }

                processedShowKeys.Add(ratingKey);

                var (isNewItem, mediaItem) =
                    await _mediaItemProcessor.GetMediaItem(ratingKey, mediaType, localMediaItems, authToken, plexUri);

                await GetChildMetadata(mediaItem, authToken, plexUri);

                _mediaItemProcessor.UpdateResult(syncResult, isNewItem, mediaItem);
            }

            return syncResult;
        }

        private async Task GetChildMetadata(PlexMediaItem mediaItem, string authToken, string plexUri)
        {
            var seasonShowItems = await GetChildShowItems(mediaItem.Key, authToken, plexUri);

            foreach (var seasonShowItem in seasonShowItems)
            {
                var plexSeason = new PlexSeason
                {
                    Key = seasonShowItem.RatingKey,
                    Title = seasonShowItem.Title,
                    AgentSourceId = seasonShowItem.AgentSourceId,
                    AgentType = seasonShowItem.AgentType,
                    Season = seasonShowItem.Index
                };

                var episodeShowItems = await GetChildShowItems(seasonShowItem.RatingKey, authToken, plexUri);

                plexSeason.Episodes = episodeShowItems.Select(ep => new PlexEpisode
                {
                    Key = ep.RatingKey,
                    Title = ep.Title,
                    AgentSourceId = ep.AgentSourceId,
                    AgentType = ep.AgentType,
                    Episode = ep.Index,
                    Year = ep.Year,
                    Resolution = ep.Resolution
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
                var childKey = Convert.ToInt32(child.RatingKey);
                showItems.Add(await CreateShowItem(childKey, authToken, plexUri));
            }

            return showItems;
        }

        private async Task<ShowMediaItem> CreateShowItem(int key, string authToken, string plexUri)
        {
            var itemInfo = await _plexApi.GetMetadata(authToken, plexUri, key);

            var metadata = itemInfo.MediaContainer.Metadata.FirstOrDefault();

            var agentDetails = _mediaItemProcessor.GetAgentDetails(metadata?.Guid);

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