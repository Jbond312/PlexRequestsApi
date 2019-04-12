using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using PlexRequests.Core;
using PlexRequests.Helpers;
using PlexRequests.Models.ViewModels;
using PlexRequests.Plex;
using PlexRequests.Store.Enums;
using PlexRequests.Store.Models;
using PlexRequests.TheMovieDb;
using PlexRequests.TheMovieDb.Models;

namespace PlexRequests.Models.Requests
{
    public class CreateTvRequestCommandHandler : AsyncRequestHandler<CreateTvRequestCommand>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly ITheMovieDbApi _theMovieDbApi;
        private readonly IPlexService _plexService;
        private readonly IClaimsPrincipalAccessor _claimsPrincipalAccessor;

        public CreateTvRequestCommandHandler(
            IMapper mapper,
            IRequestService requestService,
            ITheMovieDbApi theMovieDbApi,
            IPlexService plexService,
            IClaimsPrincipalAccessor claimsPrincipalAccessor)
        {
            _mapper = mapper;
            _requestService = requestService;
            _theMovieDbApi = theMovieDbApi;
            _plexService = plexService;
            _claimsPrincipalAccessor = claimsPrincipalAccessor;
        }

        protected override async Task Handle(CreateTvRequestCommand request, CancellationToken cancellationToken)
        {
            ValidateRequestIsCorrect(request);

            var seasons = _mapper.Map<List<RequestSeason>>(request.Seasons);

            var tvDetails = await GetTvDetails(request.TheMovieDbId);

            await ValidateDuplicateEpisodeRequests(request.TheMovieDbId, seasons);

            await ValidateRequestedEpisodesNotAlreadyInPlex(request.TheMovieDbId, seasons);

            await CreateRequest(request, seasons, tvDetails);
        }

        private async Task<TvDetails> GetTvDetails(int theMovieDbId)
        {
            return await _theMovieDbApi.GetTvDetails(theMovieDbId);
        }

        private async Task CreateRequest(CreateTvRequestCommand request, List<RequestSeason> seasons,
            TvDetails tvDetails)
        {
            var tvRequest = new Request
            {
                MediaType = PlexMediaTypes.Show,
                AgentType = AgentTypes.TheMovieDb,
                AgentSourceId = request.TheMovieDbId.ToString(),
                IsApproved = false,
                Seasons = await SetSeasonData(request.TheMovieDbId, seasons, tvDetails),
                RequestedByUserId = _claimsPrincipalAccessor.UserId,
                RequestedByUserName = _claimsPrincipalAccessor.Username,
                Title = tvDetails.Name,
                AirDate = DateTime.Parse(tvDetails.First_Air_Date),
                ImagePath = tvDetails.Poster_Path
            };

            await _requestService.Create(tvRequest);
        }

        private async Task<List<RequestSeason>> SetSeasonData(int theMovieDbId, List<RequestSeason> seasons,
            TvDetails tvDetails)
        {
            seasons = seasons.Where(x => x.Episodes.Any()).ToList();

            foreach (var season in seasons)
            {
                SetAdditionalSeasonData(tvDetails, season);

                await SetAdditionalEpisodeData(theMovieDbId, season);
            }

            return seasons;
        }

        private async Task SetAdditionalEpisodeData(int theMovieDbId, RequestSeason season)
        {
            var seasonDetails = await _theMovieDbApi.GetTvSeasonDetails(theMovieDbId, season.Season);

            foreach (var episode in season.Episodes)
            {
                var matchingEpisode =
                    seasonDetails.Episodes.FirstOrDefault(x => x.Episode_Number == episode.Episode);

                if (matchingEpisode != null)
                {
                    episode.AirDate = DateTime.Parse(matchingEpisode.Air_Date);
                    episode.Title = matchingEpisode.Name;
                    episode.ImagePath = matchingEpisode.Still_Path;
                }
            }
        }

        private async Task ValidateRequestedEpisodesNotAlreadyInPlex(int theMovieDbId, List<RequestSeason> seasons)
        {
            var plexMediaItem = await GetPlexMediaItem(theMovieDbId);

            if (plexMediaItem != null)
            {
                RemoveExistingPlexEpisodesFromRequest(seasons, plexMediaItem);

                if (!seasons.SelectMany(x => x.Episodes).Any())
                {
                    throw new PlexRequestException("Request not created",
                        "All TV Episodes are already available in Plex.");
                }
            }
        }

        private async Task<PlexMediaItem> GetPlexMediaItem(int theMovieDbId)
        {
            var plexMediaItem = await _plexService.GetExistingMediaItemByAgent(PlexMediaTypes.Show,
                AgentTypes.TheMovieDb,
                theMovieDbId.ToString());

            if (plexMediaItem != null)
            {
                return plexMediaItem;
            }

            var externalIds = await _theMovieDbApi.GetTvExternalIds(theMovieDbId);

            if (!string.IsNullOrEmpty(externalIds.TvDb_Id))
            {
                plexMediaItem = await _plexService.GetExistingMediaItemByAgent(PlexMediaTypes.Show, AgentTypes.TheTvDb,
                    externalIds.TvDb_Id);
            }

            return plexMediaItem;
        }

        private async Task ValidateDuplicateEpisodeRequests(int theMovieDbId, List<RequestSeason> seasons)
        {
            var requests =
                await _requestService.GetExistingTvRequests(AgentTypes.TheMovieDb, theMovieDbId.ToString());

            var existingSeasonEpisodeRequests = requests
                                                .SelectMany(x => x.Seasons)
                                                .GroupBy(x => x.Season)
                                                .ToDictionary(x => x.Key,
                                                    v => v.SelectMany(res =>
                                                        res.Episodes.Select(re => re.Episode)
                                                           .Distinct()
                                                    ));

            RemoveDuplicateEpisodeRequests(seasons, existingSeasonEpisodeRequests);

            if (!seasons.SelectMany(x => x.Episodes).Any())
            {
                throw new PlexRequestException("Request not created", "All TV Episodes have already been requested.");
            }
        }

        private static void SetAdditionalSeasonData(TvDetails tvDetails, RequestSeason season)
        {
            var matchingSeason = tvDetails.Seasons.FirstOrDefault(x => x.Season_Number == season.Season);

            if (matchingSeason != null)
            {
                season.AirDate = DateTime.Parse(matchingSeason.Air_Date);
                season.ImagePath = matchingSeason.Poster_Path;
            }
        }

        private static void ValidateRequestIsCorrect(CreateTvRequestCommand request)
        {
            if (request.Seasons == null || !request.Seasons.Any())
            {
                throw new PlexRequestException("Request not created",
                    "At least one season must be given in a request.");
            }

            RemoveSeasonsWithNoEpisodes(request.Seasons);

            if (!request.Seasons.Any())
            {
                throw new PlexRequestException("Request not created",
                    "Each requested season must have at least one episode.");

            }
        }

        private static void RemoveSeasonsWithNoEpisodes(List<RequestSeasonViewModel> seasons)
        {
            var emptySeasons = seasons.Where(x => x.Episodes == null || !x.Episodes.Any());

            seasons.RemoveAll(x => emptySeasons.Contains(x));
        }

        private static void RemoveDuplicateEpisodeRequests(List<RequestSeason> seasons,
            IReadOnlyDictionary<int, IEnumerable<int>> existingSeasonEpisodeRequests)
        {
            foreach (var season in seasons)
            {
                if (!existingSeasonEpisodeRequests.TryGetValue(season.Season, out var existingRequests))
                {
                    continue;
                }

                var duplicateEpisodes = season.Episodes.Where(x => existingRequests.Contains(x.Episode)).ToList();
                season.Episodes.RemoveAll(x => duplicateEpisodes.Contains(x));
            }
        }

        private static void RemoveExistingPlexEpisodesFromRequest(List<RequestSeason> seasons,
            PlexMediaItem plexMediaItem)
        {
            foreach (var season in seasons)
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