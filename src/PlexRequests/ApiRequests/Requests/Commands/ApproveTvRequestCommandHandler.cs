using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class ApproveTvRequestCommandHandler : AsyncRequestHandler<ApproveTvRequestCommand>
    {
        private readonly ITvRequestService _requestService;
        private readonly IUnitOfWork _unitOfWork;

        public ApproveTvRequestCommandHandler(
            ITvRequestService requestService,
            IUnitOfWork unitOfWork
            )
        {
            _requestService = requestService;
            _unitOfWork = unitOfWork;
        }

        protected override async Task Handle(ApproveTvRequestCommand command, CancellationToken cancellationToken)
        {
            var request = await _requestService.GetRequestById(command.RequestId);

            if (request == null)
            {
                throw new PlexRequestException("Invalid request", "No request was found with the given Id", HttpStatusCode.NotFound);
            }

            if (request.RequestStatus == RequestStatuses.Completed)
            {
                throw new PlexRequestException("Invalid request", "Request has already been completed");
            }

            if (request.Track)
            {
                request.RequestStatus = RequestStatuses.Approved;
            }
            else
            {
                if (command.ApproveAll)
                {
                    ApproveAllEpisodes(request);
                }
                else
                {
                    ApproveEpisodes(request, command.EpisodesBySeason);
                }

                _requestService.SetAggregatedStatus(request);
            }

            await _unitOfWork.CommitAsync();
        }

        private static void ApproveEpisodes(TvRequestRow request, IReadOnlyDictionary<int, List<int>> commandEpisodesBySeason)
        {
            foreach (var season in request.TvRequestSeasons)
            {
                if (!commandEpisodesBySeason.TryGetValue(season.SeasonIndex, out var commandEpisodes))
                {
                    continue;
                }

                foreach (var episode in season.TvRequestEpisodes)
                {
                    if (!commandEpisodes.Contains(episode.EpisodeIndex))
                    {
                        continue;
                    }

                    ApproveEpisode(episode);
                }
            }
        }

        private static void ApproveAllEpisodes(TvRequestRow request)
        {
            foreach (var season in request.TvRequestSeasons)
            {
                foreach (var episode in season.TvRequestEpisodes)
                {
                    ApproveEpisode(episode);
                }
            }
        }

        private static void ApproveEpisode(TvRequestEpisodeRow episode)
        {
            if (episode.RequestStatus != RequestStatuses.Completed)
            {
                episode.RequestStatus = RequestStatuses.Approved;
            }
        }
    }
}