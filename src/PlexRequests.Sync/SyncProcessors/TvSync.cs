using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlexRequests.Plex;
using PlexRequests.Store.Enums;
using PlexRequests.Store.Models;

namespace PlexRequests.Sync.SyncProcessors
{
    public class TvSync : BaseMediaSync
    {
        private readonly IPlexApi _plexApi;

        public TvSync(
            IPlexApi plexApi,
            IPlexService plexService,
            ILogger logger,
            string authToken,
            string plexUri
        ) : base(plexApi, plexService, logger, authToken, plexUri)
        {
            _plexApi = plexApi;
        }

        protected override PlexMediaTypes MediaType => PlexMediaTypes.Show;

        protected override async Task GetChildMetadata(PlexMediaItem mediaItem)
        {
            var seasonShowItems = await GetChildShowItems(mediaItem.Key);

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

                var episodeShowItems = await GetChildShowItems(seasonShowItem.RatingKey);

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

        private async Task<List<ShowMediaItem>> GetChildShowItems(int parentKey)
        {
            var showItems = new List<ShowMediaItem>();
            var childContainer = await _plexApi.GetChildrenMetadata(AuthToken, PlexUri, parentKey);

            foreach (var child in childContainer.MediaContainer.Metadata)
            {
                var childKey = Convert.ToInt32(child.RatingKey);
                showItems.Add(await CreateShowItem(childKey));
            }

            return showItems;
        }

        private async Task<ShowMediaItem> CreateShowItem(int key)
        {
            var itemInfo = await _plexApi.GetMetadata(AuthToken, PlexUri, key);

            var metadata = itemInfo.MediaContainer.Metadata.FirstOrDefault();

            var agentDetails = GetAgentDetails(metadata?.Guid);

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
