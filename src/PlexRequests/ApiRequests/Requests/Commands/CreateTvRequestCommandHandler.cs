using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using PlexRequests.ApiRequests.Requests.Models.Create;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using PlexRequests.TheMovieDb;
using PlexRequests.TheMovieDb.Models;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class CreateTvRequestCommandHandler : AsyncRequestHandler<CreateTvRequestCommand>
    {
        private readonly IMapper _mapper;
        private readonly ITvRequestService _requestService;
        private readonly ITheMovieDbService _theMovieDbService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsPrincipalAccessor _claimsPrincipalAccessor;

        public CreateTvRequestCommandHandler(
            IMapper mapper,
            ITvRequestService requestService,
            ITheMovieDbService theMovieDbService,
            IUnitOfWork unitOfWork,
            IClaimsPrincipalAccessor claimsPrincipalAccessor)
        {
            _mapper = mapper;
            _requestService = requestService;
            _theMovieDbService = theMovieDbService;
            _unitOfWork = unitOfWork;
            _claimsPrincipalAccessor = claimsPrincipalAccessor;
        }

        protected override async Task Handle(CreateTvRequestCommand request, CancellationToken cancellationToken)
        {
             ValidateRequestIsCorrect(request);

             var seasonsToRequest = _mapper.Map<List<TvRequestSeasonRow>>(request.Seasons);

             var existingRequest = await _requestService.GetExistingRequest(AgentTypes.TheMovieDb, request.TheMovieDbId.ToString());
             
             var existingUserRequests = new List<TvRequestUserRow>();
             if (existingRequest != null)
             {
                 existingUserRequests = existingRequest.TvRequestUsers.Where(x => x.UserId == _claimsPrincipalAccessor.UserId).ToList();
             }

             var tvDetails = await GetTvDetails(request.TheMovieDbId);

             existingRequest ??= await CreateRequest(request);

             if (request.TrackShow)
             {
                 if (!tvDetails.In_Production)
                 {
                     throw new PlexRequestException("Request not created", "Cannot track a TV Show that is no longer in production");
                 }

                 ValidateShowIsntAlreadyTracked(existingUserRequests);

                 if (!existingRequest.Track)
                 {
                     existingRequest.Track = true;
                 }

                 existingRequest.TvRequestUsers.Add(new TvRequestUserRow
                 {
                     Track = true,
                     UserId = _claimsPrincipalAccessor.UserId
                 });
             }
             else
             {
                 ValidateAndRemoveExistingEpisodeRequests(existingUserRequests, seasonsToRequest);
                 ValidateRequestedEpisodesNotAlreadyInPlex(existingRequest.TvRequestSeasons, seasonsToRequest);
                 await AddNewRootLevelRequests(existingRequest, seasonsToRequest);
                 AddUserRequests(existingRequest, seasonsToRequest);
             }
             
             await _unitOfWork.CommitAsync();
        }

        private void AddUserRequests(TvRequestRow existingRequest, List<TvRequestSeasonRow> seasonsToRequest)
        {
            var userId = _claimsPrincipalAccessor.UserId;
            foreach (var seasonToRequest in seasonsToRequest)
            {
                var userRequest = new TvRequestUserRow
                {
                    UserId = userId,
                    Season = seasonToRequest.SeasonIndex
                };

                foreach(var episodeToRequest in seasonToRequest.TvRequestEpisodes)
                {
                    userRequest.Episode = episodeToRequest.EpisodeIndex;
                }

                existingRequest.TvRequestUsers.Add(userRequest);
            }
        }

        private async Task AddNewRootLevelRequests(TvRequestRow existingRequest, List<TvRequestSeasonRow> seasonsToRequest)
        {
            foreach (var seasonToRequest in seasonsToRequest)
            {
                var seasonDetails = await _theMovieDbService.GetTvSeasonDetails(existingRequest.TheMovieDbId, seasonToRequest.SeasonIndex);
                var rootSeason = existingRequest.TvRequestSeasons.FirstOrDefault(x => x.SeasonIndex == seasonToRequest.SeasonIndex);
                if (seasonDetails != null && rootSeason == null)
                {
                    rootSeason = new TvRequestSeasonRow
                    {
                        AirDateUtc = DateTime.Parse(seasonDetails.Air_Date),
                        Title = seasonDetails.Name,
                        ImagePath = seasonDetails.Poster_Path,
                        RequestStatus = RequestStatuses.PendingApproval
                    };
                }

                SetEpisodeDetails(rootSeason, seasonToRequest.TvRequestEpisodes, seasonDetails);

                existingRequest.TvRequestSeasons.Add(rootSeason);
            }
        }

        private static void SetEpisodeDetails(TvRequestSeasonRow rootSeason, ICollection<TvRequestEpisodeRow> tvRequestEpisodes, TvSeasonDetails seasonDetails)
        {
            foreach (var episodeToRequest in tvRequestEpisodes)
            {
                var rootEpisode = rootSeason.TvRequestEpisodes.FirstOrDefault(x => x.EpisodeIndex == episodeToRequest.EpisodeIndex);
                var episodeDetail = seasonDetails.Episodes.FirstOrDefault(x => x.Episode_Number == episodeToRequest.EpisodeIndex);

                if (episodeDetail == null || rootEpisode != null)
                {
                    continue;
                }

                rootEpisode = new TvRequestEpisodeRow
                {
                    AirDateUtc = DateTime.Parse(episodeDetail.Air_Date),
                    Title = episodeDetail.Name,
                    ImagePath = episodeDetail.Still_Path,
                    RequestStatus = RequestStatuses.PendingApproval
                };
                rootSeason.TvRequestEpisodes.Add(rootEpisode);
            }
        }
        
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static void ValidateShowIsntAlreadyTracked(IEnumerable<TvRequestUserRow> userRequests)
        {
            if (userRequests.Any(x => x.Track))
            {
                throw new PlexRequestException("Request not created", "TV Show is already being tracked");
            }
        }

        private async Task<TvDetails> GetTvDetails(int theMovieDbId)
        {
            return await _theMovieDbService.GetTvDetails(theMovieDbId);
        }

        private async Task<TvRequestRow> CreateRequest(CreateTvRequestCommand request)
        {
            var tvDetails = _theMovieDbService.GetTvDetails(request.TheMovieDbId);
            var externalIds = _theMovieDbService.GetTvExternalIds(request.TheMovieDbId);

            await Task.WhenAll(tvDetails, externalIds);

            var tvRequest = new TvRequestRow
            {
                RequestStatus = RequestStatuses.PendingApproval,
                Title = tvDetails.Result.Name,
                AirDateUtc = DateTime.Parse(tvDetails.Result.First_Air_Date),
                ImagePath = tvDetails.Result.Poster_Path,
                Track = request.TrackShow,
                TheMovieDbId = request.TheMovieDbId
            };

            tvRequest.TvRequestAgents.Add(new TvRequestAgentRow(AgentTypes.TheMovieDb, request.TheMovieDbId.ToString()));

            if (!string.IsNullOrEmpty(externalIds.Result.TvDb_Id))
            {
                tvRequest.TvRequestAgents.Add(new TvRequestAgentRow(AgentTypes.TheTvDb, externalIds.Result.TvDb_Id));
            }

            await _requestService.Add(tvRequest);

            return tvRequest;
        }

        private static void ValidateRequestedEpisodesNotAlreadyInPlex(ICollection<TvRequestSeasonRow> existingRootSeasons, List<TvRequestSeasonRow> requestedSeasons)
        {
            RemoveExistingPlexEpisodesFromRequest(existingRootSeasons, requestedSeasons);

            if (!requestedSeasons.SelectMany(x => x.TvRequestEpisodes).Any())
            {
                throw new PlexRequestException(
                    "Request not created",
                    "All TV Episodes are already available in Plex.");
            }
        }

        private static void ValidateAndRemoveExistingEpisodeRequests(List<TvRequestUserRow> existingRequests, List<TvRequestSeasonRow> seasonsToRequest)
        {
            var existingSeasonEpisodeRequests = existingRequests
                .Where(x => x.Season != null)
                .GroupBy(x => x.Season.Value)
                // ReSharper disable once PossibleInvalidOperationException
                .ToDictionary(x => x.Key, x => x.Where(s => s.Episode != null).Select(e => e.Episode.Value));

            RemoveDuplicateEpisodeRequests(seasonsToRequest, existingSeasonEpisodeRequests);

            if (!seasonsToRequest.SelectMany(x => x.TvRequestEpisodes).Any())
            {
                throw new PlexRequestException("Request not created", "All TV Episodes have already been requested.");
            }
        }

        private static void ValidateRequestIsCorrect(CreateTvRequestCommand request)
        {
            if (request.TrackShow && request.Seasons != null && request.Seasons.Count > 0)
            {
                throw new PlexRequestException("Request not created", "Requests to track and for specific episodes must be made separately.");
            }

            if (!request.TrackShow)
            {
                if (request.Seasons == null || !request.Seasons.Any())
                {
                    throw new PlexRequestException("Request not created",
                        "At least one season must be given in a request.");
                }

                ValidateNoDuplicateSeasonsOrEpisodes(request);

                RemoveSeasonsWithNoEpisodes(request.Seasons);

                if (!request.Seasons.Any())
                {
                    throw new PlexRequestException("Request not created",
                        "Each requested season must have at least one episode.");

                }
            }
        }

        private static void ValidateNoDuplicateSeasonsOrEpisodes(CreateTvRequestCommand request)
        {
            var existingSeasons = new List<int>();
            for (var sIndex = 0; sIndex < request.Seasons?.Count; sIndex++)
            {
                var season = request.Seasons[sIndex];

                if (existingSeasons.Contains(season.Index))
                {
                    throw new PlexRequestException("Request not created", "All seasons in a request must be unique.");
                }

                existingSeasons.Add(season.Index);

                var existingEpisodes = new List<int>();
                for (var eIndex = 0; eIndex < season.Episodes?.Count; eIndex++)
                {
                    var episode = season.Episodes[eIndex];
                    if (existingEpisodes.Contains(episode.Index))
                    {
                        throw new PlexRequestException("Request not created", "All episodes in a season must be unique.");
                    }

                    existingEpisodes.Add(episode.Index);
                }
            }
        }

        private static void RemoveSeasonsWithNoEpisodes(List<TvRequestSeasonCreateModel> seasons)
        {
            var emptySeasons = seasons.Where(x => x.Episodes == null || !x.Episodes.Any());

            seasons.RemoveAll(x => emptySeasons.Contains(x));
        }

        private static void RemoveDuplicateEpisodeRequests(IEnumerable<TvRequestSeasonRow> seasons,
            IReadOnlyDictionary<int, IEnumerable<int>> existingSeasonEpisodeRequests)
        {
            foreach (var season in seasons)
            {
                if (!existingSeasonEpisodeRequests.TryGetValue(season.SeasonIndex, out var existingRequests))
                {
                    continue;
                }

                var duplicateEpisodes = season.TvRequestEpisodes.Where(x => existingRequests.Contains(x.EpisodeIndex)).ToList();

               foreach(var duplicateEpisode in duplicateEpisodes)
               {
                   season.TvRequestEpisodes.Remove(duplicateEpisode);
               }
            }
        }

        private static void RemoveExistingPlexEpisodesFromRequest(ICollection<TvRequestSeasonRow> existingRootSeasons, List<TvRequestSeasonRow> requestedSeasons)
        {
            var requestedSeasonToRemove = new List<TvRequestSeasonRow>();
            foreach (var requestedSeason in requestedSeasons)
            {
                var matchingExistingSeason = existingRootSeasons.FirstOrDefault(x => x.SeasonIndex == requestedSeason.SeasonIndex);

                if (matchingExistingSeason == null)
                {
                    continue;
                }

                var requestedEpisodesToRemove = new List<TvRequestEpisodeRow>();
                foreach (var requestedEpisode in requestedSeason.TvRequestEpisodes)
                {
                    var matchingExistingEpisode = matchingExistingSeason.TvRequestEpisodes.FirstOrDefault(x => x.EpisodeIndex == requestedEpisode.EpisodeIndex);

                    if (matchingExistingEpisode?.PlexEpisode != null)
                    {
                        requestedEpisodesToRemove.Add(requestedEpisode);
                    }
                }

                if (requestedEpisodesToRemove.Any())
                {
                    foreach (var episodeToRemove in requestedEpisodesToRemove)
                    {
                        requestedSeason.TvRequestEpisodes.Remove(episodeToRemove);
                    }
                }

                if (!requestedSeason.TvRequestEpisodes.Any())
                {
                    requestedSeasonToRemove.Add(requestedSeason);
                }
            }

            foreach (var seasonToRemove in requestedSeasonToRemove)
            {
                requestedSeasons.Remove(seasonToRemove);
            }
        }
    }
}