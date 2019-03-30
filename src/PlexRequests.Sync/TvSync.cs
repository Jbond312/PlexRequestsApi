using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlexRequests.Plex;
using PlexRequests.Store.Enums;
using PlexRequests.Store.Models;

namespace PlexRequests.Sync
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
            var seasonContainer = await _plexApi.GetChildrenMetadata(AuthToken, PlexUri, mediaItem.Key);
            foreach (var season in seasonContainer.MediaContainer.Metadata)
            {
                var mediaSeason = mediaItem.Seasons.FirstOrDefault(x => x.Season == season.Index);

                var seasonRatingKey = Convert.ToInt32(season.RatingKey);
                
                if (mediaSeason == null)
                {
                    mediaSeason = new PlexSeason
                    {
                        Key = seasonRatingKey,
                        Title = season.Title,
                        Season = season.Index
                    };

                    mediaItem.Seasons.Add(mediaSeason);
                }

                var seasonInfoContainer = await _plexApi.GetMetadata(AuthToken, PlexUri, seasonRatingKey);
                var seasonGuid = seasonInfoContainer.MediaContainer.Metadata.FirstOrDefault()?.Guid;

                SetAgentDetails(mediaSeason, seasonGuid);

                var episodeContainer = await _plexApi.GetChildrenMetadata(AuthToken, PlexUri, seasonRatingKey);

                foreach (var episode in episodeContainer.MediaContainer.Metadata)
                {
                    var mediaEpisode = mediaSeason.Episodes.FirstOrDefault(x => x.Episode == episode.Index);

                    var episodeRatingKey = Convert.ToInt32(episode.RatingKey);

                    var episodeInfoContainer = await _plexApi.GetMetadata(AuthToken, PlexUri, episodeRatingKey);

                    if (mediaEpisode == null)
                    {
                        mediaEpisode = new PlexEpisode
                        {
                            Key = episodeRatingKey,
                            Title = episode.Title,
                            Episode = episode.Index,
                            Year = episodeInfoContainer.MediaContainer.Metadata.FirstOrDefault()?.Year ?? 0
                        };

                        mediaSeason.Episodes.Add(mediaEpisode);
                    }

                    var episodeGuid = seasonInfoContainer.MediaContainer.Metadata.FirstOrDefault()?.Guid;

                    SetAgentDetails(mediaEpisode, episodeGuid);
                }
            }
        }
    }
}
