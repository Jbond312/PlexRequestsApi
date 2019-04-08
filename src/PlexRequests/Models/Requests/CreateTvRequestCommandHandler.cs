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
                SeasonEpisodes = ConvertToRequestEpisodes(request.SeasonEpisodes),
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

                if (!request.SeasonEpisodes.SelectMany(x => x.Value).Any())
                {
                    throw new PlexRequestException("Request not created",
                        "All TV Episodes hare already available in Plex.");
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
                                                .SelectMany(x => x.SeasonEpisodes)
                                                .GroupBy(x => x.Key)
                                                .ToDictionary(x => x.Key,
                                                    v => v.SelectMany(res =>
                                                        res.Value.Select(re => re.Episode)
                                                           .Distinct()
                                                    ));

            RemoveDuplicateEpisodeRequests(request, existingSeasonEpisodeRequests);

            if (!request.SeasonEpisodes.SelectMany(x => x.Value).Any())
            {
                throw new PlexRequestException("Request not created", "All TV Episodes have already been requested.");
            }
        }

        private static void RemoveDuplicateEpisodeRequests(CreateTvRequestCommand request,
            IReadOnlyDictionary<int, IEnumerable<int>> existingSeasonEpisodeRequests)
        {
            foreach (var (season, episodes) in request.SeasonEpisodes)
            {
                if (!existingSeasonEpisodeRequests.TryGetValue(season, out var existingRequests))
                {
                    continue;
                }

                var duplicateEpisodes = episodes.Where(x => existingRequests.Contains(x)).ToList();
                episodes.RemoveAll(x => duplicateEpisodes.Contains(x));
            }
        }

        private static Dictionary<int, List<RequestEpisode>> ConvertToRequestEpisodes(
            Dictionary<int, List<int>> seasonEpisodes)
        {
            var seasonEpisodeRequests = new Dictionary<int, List<RequestEpisode>>();
            foreach (var (season, episodes) in seasonEpisodes)
            {
                if (!episodes.Any())
                {
                    continue;
                }

                seasonEpisodeRequests.Add(season, episodes.Select(episode => new RequestEpisode
                {
                    Episode = episode,
                    IsApproved = false
                }).ToList());
            }

            return seasonEpisodeRequests;
        }

        private static void RemoveExistingPlexEpisodesFromRequest(CreateTvRequestCommand request,
            PlexMediaItem plexMediaItem)
        {
            foreach (var (key, value) in request.SeasonEpisodes)
            {
                var matchingSeason = plexMediaItem.Seasons.FirstOrDefault(x => x.Season == key);

                if (matchingSeason == null)
                {
                    continue;
                }

                var matchingEpisodes = matchingSeason
                                       .Episodes.Where(x => value.Contains(x.Episode)).Select(x => x.Episode)
                                       .ToList();

                if (matchingEpisodes.Any())
                {
                    value.RemoveAll(x => matchingEpisodes.Contains(x));
                }
            }
        }
    }
}