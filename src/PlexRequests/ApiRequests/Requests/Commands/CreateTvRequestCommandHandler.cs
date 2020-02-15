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
using PlexRequests.Plex;
using PlexRequests.TheMovieDb;
using PlexRequests.TheMovieDb.Models;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class CreateTvRequestCommandHandler : AsyncRequestHandler<CreateTvRequestCommand>
    {
        private readonly IMapper _mapper;
        private readonly ITvRequestService _requestService;
        private readonly ITheMovieDbService _theMovieDbService;
        private readonly IPlexService _plexService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsPrincipalAccessor _claimsPrincipalAccessor;

        public CreateTvRequestCommandHandler(
            IMapper mapper,
            ITvRequestService requestService,
            ITheMovieDbService theMovieDbService,
            IPlexService plexService,
            IUnitOfWork unitOfWork,
            IClaimsPrincipalAccessor claimsPrincipalAccessor)
        {
            _mapper = mapper;
            _requestService = requestService;
            _theMovieDbService = theMovieDbService;
            _plexService = plexService;
            _unitOfWork = unitOfWork;
            _claimsPrincipalAccessor = claimsPrincipalAccessor;
        }

        protected override async Task Handle(CreateTvRequestCommand request, CancellationToken cancellationToken)
        {
            ValidateRequestIsCorrect(request);

            var seasons = _mapper.Map<List<TvRequestSeasonRow>>(request.Seasons);

            var tvDetails = await GetTvDetails(request.TheMovieDbId);

            var externalIds = await _theMovieDbService.GetTvExternalIds(request.TheMovieDbId);

            if (request.TrackShow)
            {
                if (!tvDetails.In_Production)
                {
                    throw new PlexRequestException("Request not created", "Cannot track a TV Show that is no longer in production");
                }

                await ValidateShowIsntAlreadyTracked(request.TheMovieDbId);
            }
            else
            {
                await ValidateAndRemoveExistingEpisodeRequests(request.TheMovieDbId, seasons);

                await ValidateRequestedEpisodesNotAlreadyInPlex(request.TheMovieDbId, seasons, externalIds);
            }

            await CreateRequest(request, seasons, tvDetails, externalIds);

            await _unitOfWork.CommitAsync();
        }

        private async Task ValidateShowIsntAlreadyTracked(int theMovieDbId)
        {
            var existingRequests = await _requestService.GetExistingRequests(AgentTypes.TheMovieDb, theMovieDbId.ToString());

            if (existingRequests.Any(x => x.Track))
            {
                throw new PlexRequestException("Request not created", "TV Show is already being tracked");
            }
        }

        private async Task<TvDetails> GetTvDetails(int theMovieDbId)
        {
            return await _theMovieDbService.GetTvDetails(theMovieDbId);
        }

        private async Task CreateRequest(CreateTvRequestCommand request, List<TvRequestSeasonRow> seasons,
            TvDetails tvDetails, ExternalIds externalIds)
        {
            var tvRequest = new TvRequestRow
            {
                RequestStatus = RequestStatuses.PendingApproval,
                TvRequestSeasons = await SetSeasonData(request.TheMovieDbId, seasons, tvDetails),
                //TODO This command needs re-writing due to changes to how we store tv requests for users
                //RequestedByUserId = _claimsPrincipalAccessor.UserId,
                Title = tvDetails.Name,
                AirDateUtc = DateTime.Parse(tvDetails.First_Air_Date),
                ImagePath = tvDetails.Poster_Path,
                Track = request.TrackShow
            };

            tvRequest.TvRequestAgents.Add(new TvRequestAgentRow(AgentTypes.TheMovieDb, request.TheMovieDbId.ToString()));

            if (!string.IsNullOrEmpty(externalIds.TvDb_Id))
            {
                tvRequest.TvRequestAgents.Add(new TvRequestAgentRow(AgentTypes.TheTvDb, externalIds.TvDb_Id));
            }

            await _requestService.Add(tvRequest);
        }

        private async Task<List<TvRequestSeasonRow>> SetSeasonData(int theMovieDbId, List<TvRequestSeasonRow> seasons,
            TvDetails tvDetails)
        {
            if (seasons == null)
            {
                return new List<TvRequestSeasonRow>();
            }

            seasons = seasons.Where(x => x.TvRequestEpisodes.Any()).ToList();

            foreach (var season in seasons)
            {
                SetAdditionalSeasonData(tvDetails, season);

                await SetAdditionalEpisodeData(theMovieDbId, season);
            }

            return seasons;
        }

        private async Task SetAdditionalEpisodeData(int theMovieDbId, TvRequestSeasonRow season)
        {
            var seasonDetails = await _theMovieDbService.GetTvSeasonDetails(theMovieDbId, season.SeasonIndex);

            season.TvRequestEpisodes ??= new List<TvRequestEpisodeRow>();

            foreach (var episode in season.TvRequestEpisodes)
            {
                var matchingEpisode =
                    seasonDetails.Episodes.FirstOrDefault(x => x.Episode_Number == episode.EpisodeIndex);

                if (matchingEpisode != null)
                {
                    episode.AirDateUtc = DateTime.Parse(matchingEpisode.Air_Date);
                    episode.Title = matchingEpisode.Name;
                    episode.ImagePath = matchingEpisode.Still_Path;
                    episode.RequestStatus = RequestStatuses.PendingApproval;
                }
                else
                {
                    episode.RequestStatus = RequestStatuses.Rejected;
                }
            }
        }

        private async Task ValidateRequestedEpisodesNotAlreadyInPlex(int theMovieDbId, List<TvRequestSeasonRow> seasons, ExternalIds externalIds)
        {
            var plexMediaItem = await GetPlexMediaItem(theMovieDbId, externalIds);

            if (plexMediaItem != null)
            {
                RemoveExistingPlexEpisodesFromRequest(seasons, plexMediaItem);

                if (!seasons.SelectMany(x => x.TvRequestEpisodes).Any())
                {
                    throw new PlexRequestException("Request not created",
                        "All TV Episodes are already available in Plex.");
                }
            }
        }

        private async Task<PlexMediaItemRow> GetPlexMediaItem(int theMovieDbId, ExternalIds externalIds)
        {
            var plexMediaItem = await _plexService.GetExistingMediaItemByAgent(PlexMediaTypes.Show,
                AgentTypes.TheMovieDb,
                theMovieDbId.ToString());

            if (plexMediaItem != null)
            {
                return plexMediaItem;
            }

            if (!string.IsNullOrEmpty(externalIds.TvDb_Id))
            {
                plexMediaItem = await _plexService.GetExistingMediaItemByAgent(PlexMediaTypes.Show, AgentTypes.TheTvDb,
                    externalIds.TvDb_Id);
            }

            return plexMediaItem;
        }

        private async Task ValidateAndRemoveExistingEpisodeRequests(int theMovieDbId, List<TvRequestSeasonRow> seasons)
        {
            var requests =
                await _requestService.GetExistingRequests(AgentTypes.TheMovieDb, theMovieDbId.ToString());

            var existingSeasonEpisodeRequests = requests
                                                .SelectMany(x => x.TvRequestSeasons)
                                                .GroupBy(x => x.SeasonIndex)
                                                .ToDictionary(x => x.Key,
                                                    v => v.SelectMany(res =>
                                                        res.TvRequestEpisodes.Select(re => re.EpisodeIndex)
                                                           .Distinct()
                                                    ));

            RemoveDuplicateEpisodeRequests(seasons, existingSeasonEpisodeRequests);

            if (!seasons.SelectMany(x => x.TvRequestEpisodes).Any())
            {
                throw new PlexRequestException("Request not created", "All TV Episodes have already been requested.");
            }
        }

        private static void SetAdditionalSeasonData(TvDetails tvDetails, TvRequestSeasonRow season)
        {
            var matchingSeason = tvDetails.Seasons.FirstOrDefault(x => x.Season_Number == season.SeasonIndex);

            if (matchingSeason != null)
            {
                season.AirDateUtc = DateTime.Parse(matchingSeason.Air_Date);
                season.ImagePath = matchingSeason.Poster_Path;
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

        private static void RemoveDuplicateEpisodeRequests(List<TvRequestSeasonRow> seasons,
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

        private static void RemoveExistingPlexEpisodesFromRequest(List<TvRequestSeasonRow> seasons,
            PlexMediaItemRow plexMediaItem)
        {
            foreach (var season in seasons)
            {
                var matchingSeason = plexMediaItem.PlexSeasons.FirstOrDefault(x => x.Season == season.SeasonIndex);

                if (matchingSeason == null)
                {
                    continue;
                }

                var episodesToRemove = new List<TvRequestEpisodeRow>();
                foreach (var episode in matchingSeason.PlexEpisodes)
                {
                    var episodeInRequest = season.TvRequestEpisodes.FirstOrDefault(x => x.EpisodeIndex == episode.Episode);

                    if (episodeInRequest != null)
                    {
                        episodesToRemove.Add(episodeInRequest);
                    }
                }

                if (episodesToRemove.Any())
                {
                    foreach (var episodeToRemove in episodesToRemove)
                    {
                        season.TvRequestEpisodes.Remove(episodeToRemove);
                    }
                }
            }
        }
    }
}