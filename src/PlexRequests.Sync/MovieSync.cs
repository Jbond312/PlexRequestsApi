using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlexRequests.Plex;
using PlexRequests.Store.Enums;
using PlexRequests.Store.Models;

namespace PlexRequests.Sync
{
    public class MovieSync : IMediaSync
    {
        private readonly IPlexApi _plexApi;
        private readonly IPlexService _plexService;
        private readonly ILogger _logger;

        public MovieSync(
            IPlexApi plexApi,
            IPlexService plexService,
            ILogger logger
            )
        {
            _plexApi = plexApi;
            _plexService = plexService;
            _logger = logger;
        }

        public async Task<SyncResult> SyncMedia(PlexServerLibrary library, string plexUri, string accessToken)
        {
            _logger.LogInformation("Synchronising Movies");

            var result = new SyncResult();

            var movieContainer = await _plexApi.GetLibrary(accessToken, plexUri, library.Key);
            var previouslySyncedMovies = await _plexService.GetMediaItems(PlexMediaTypes.Movie);

            var mediaTypeSources = Enum.GetValues(typeof(AgentTypes)).Cast<AgentTypes>().ToList();

            foreach (var movie in movieContainer.MediaContainer.Metadata)
            {
                var ratingKey = Convert.ToInt32(movie.RatingKey);

                var mediaItem = previouslySyncedMovies.FirstOrDefault(x => x.Key == ratingKey);

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
                mediaItem.MediaType = PlexMediaTypes.Movie;
                mediaItem.Title = movie.Title;
                mediaItem.Year = movie.Year;

                var metadataContainer = await _plexApi.GetMetadata(accessToken, plexUri, mediaItem.Key);

                var media = movie.Media.FirstOrDefault();
                var movieMetadata = metadataContainer.MediaContainer.Metadata.FirstOrDefault();

                mediaItem.Resolution = media?.VideoResolution;
                var agentGuid = movieMetadata?.Guid;

                if (!string.IsNullOrEmpty(agentGuid))
                {
                    var sourceSplit = agentGuid.Split(new[] {'.', ':', '/', '?'},
                        StringSplitOptions.RemoveEmptyEntries);
                    var agentTypeRaw = sourceSplit[3];
                    var idRaw = sourceSplit[4];

                    var matchingSource = mediaTypeSources.First(x =>
                        x.ToString().Contains(agentTypeRaw, StringComparison.CurrentCultureIgnoreCase));

                    mediaItem.AgentType = matchingSource;
                    mediaItem.AgentSourceId = idRaw;

                    //TODO If we end up splitting TV Shows separately we can get the season and ep number like this
                    if (matchingSource == AgentTypes.TheTvDb)
                    {
                        var season = sourceSplit[5];
                        var episode = sourceSplit[6];
                    }
                }
            }

            return result;
        }
    }
}
