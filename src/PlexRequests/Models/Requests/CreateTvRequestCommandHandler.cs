using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Core;
using PlexRequests.Helpers;
using PlexRequests.Plex;
using PlexRequests.Store.Enums;
using PlexRequests.Store.Models;
using PlexRequests.TheMovieDb;

namespace PlexRequests.Models.Requests
{
    public class CreateTvRequestCommandHandler : AsyncRequestHandler<CreateTvRequestCommand>
    {
        private readonly IRequestService _requestService;
        private readonly ITheMovieDbApi _theMovieDbApi;
        private readonly IPlexService _plexService;
        private readonly IClaimsPrincipalAccessor _claimsPrincipalAccessor;

        public CreateTvRequestCommandHandler(IRequestService requestService,
            ITheMovieDbApi theMovieDbApi,
            IPlexService plexService,
            IClaimsPrincipalAccessor claimsPrincipalAccessor)
        {
            _requestService = requestService;
            _theMovieDbApi = theMovieDbApi;
            _plexService = plexService;
            _claimsPrincipalAccessor = claimsPrincipalAccessor;
        }

        protected override async Task Handle(CreateTvRequestCommand request, CancellationToken cancellationToken)
        {
            await ValidateDuplicateEpisodeRequests(request);

            await ValidateRequestedEpisodesNotAlreadyInPlex(request);

            await CreateRequest(request);
        }

        private async Task CreateRequest(CreateTvRequestCommand request)
        {
            var tvRequest = new Request
            {
                MediaType = PlexMediaTypes.Show,
                AgentType = AgentTypes.TheMovieDb,
                AgentSourceId = request.TheMovieDbId.ToString(),
                IsApproved = false,
                Seasons = request.Seasons.Where(x => x.Episodes.Any()).ToList(),
                RequestedByUserId = _claimsPrincipalAccessor.UserId,
                RequestedByUserName = _claimsPrincipalAccessor.Username
            };

            await _requestService.Create(tvRequest);
        }

        private async Task ValidateRequestedEpisodesNotAlreadyInPlex(CreateTvRequestCommand request)
        {
            var plexMediaItem = await GetPlexMediaItem(request);

            if (plexMediaItem != null)
            {
                RemoveExistingPlexEpisodesFromRequest(request, plexMediaItem);

                if (!request.Seasons.SelectMany(x => x.Episodes).Any())
                {
                    throw new PlexRequestException("Request not created",
                        "All TV Episodes are already available in Plex.");
                }
            }
        }

        private async Task<PlexMediaItem> GetPlexMediaItem(CreateTvRequestCommand request)
        {
            var plexMediaItem = await _plexService.GetExistingMediaItemByAgent(PlexMediaTypes.Show,
                AgentTypes.TheMovieDb,
                request.TheMovieDbId.ToString());

            if (plexMediaItem != null)
            {
                return plexMediaItem;
            }

            var externalIds = await _theMovieDbApi.GetTvExternalIds(request.TheMovieDbId);

            if (!string.IsNullOrEmpty(externalIds.TvDb_Id))
            {
                plexMediaItem = await _plexService.GetExistingMediaItemByAgent(PlexMediaTypes.Show, AgentTypes.TheTvDb,
                    externalIds.TvDb_Id);
            }

            return plexMediaItem;
        }

        private async Task ValidateDuplicateEpisodeRequests(CreateTvRequestCommand request)
        {
            var requests =
                await _requestService.GetExistingTvRequests(AgentTypes.TheMovieDb, request.TheMovieDbId.ToString());

            var existingSeasonEpisodeRequests = requests
                                                .SelectMany(x => x.Seasons)
                                                .GroupBy(x => x.Season)
                                                .ToDictionary(x => x.Key,
                                                    v => v.SelectMany(res =>
                                                        res.Episodes.Select(re => re.Episode)
                                                           .Distinct()
                                                    ));

            RemoveDuplicateEpisodeRequests(request, existingSeasonEpisodeRequests);

            if (!request.Seasons.SelectMany(x => x.Episodes).Any())
            {
                throw new PlexRequestException("Request not created", "All TV Episodes have already been requested.");
            }
        }

        private static void RemoveDuplicateEpisodeRequests(CreateTvRequestCommand request,
            IReadOnlyDictionary<int, IEnumerable<int>> existingSeasonEpisodeRequests)
        {
            foreach (var season in request.Seasons)
            {
                if (!existingSeasonEpisodeRequests.TryGetValue(season.Season, out var existingRequests))
                {
                    continue;
                }

                var duplicateEpisodes = season.Episodes.Where(x => existingRequests.Contains(x.Episode)).ToList();
                season.Episodes.RemoveAll(x => duplicateEpisodes.Contains(x));
            }
        }

        private static void RemoveExistingPlexEpisodesFromRequest(CreateTvRequestCommand request,
            PlexMediaItem plexMediaItem)
        {
            foreach (var season in request.Seasons)
            {
                var matchingSeason = plexMediaItem.Seasons.FirstOrDefault(x => x.Season == season.Season);

                if (matchingSeason == null)
                {
                    continue;
                }

                var episodesToRemove = new List<RequestEpisode>();
                foreach (var episode in matchingSeason.Episodes)
                {
                    var episodeInRequest = season.Episodes.FirstOrDefault(x => x.Episode == episode.Episode);

                    if (episodeInRequest != null)
                    {
                        episodesToRemove.Add(episodeInRequest);
                    }
                }
                
                if (episodesToRemove.Any())
                {
                    season.Episodes.RemoveAll(x => episodesToRemove.Contains(x));
                }
            }
        }
    }
}