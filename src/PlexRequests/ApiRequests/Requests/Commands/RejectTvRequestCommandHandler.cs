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
    public class RejectTvRequestCommandHandler : AsyncRequestHandler<RejectTvRequestCommand>
    {
        private readonly ITvRequestService _requestService;
        private readonly IUnitOfWork _unitOfWork;

        public RejectTvRequestCommandHandler(
            ITvRequestService requestService,
            IUnitOfWork unitOfWork)
        {
            _requestService = requestService;
            _unitOfWork = unitOfWork;
        }

        protected override async Task Handle(RejectTvRequestCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.Comment))
            {
                throw new PlexRequestException("Invalid request", "A comment must be specified when rejecting a request");
            }

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
                request.RequestStatus = RequestStatuses.Rejected;
            }
            else
            {
                if (command.RejectAll)
                {
                    RejectAll(request);
                }
                else
                {
                    PartialReject(request, command.EpisodesBySeason);
                }

                _requestService.SetAggregatedStatus(request);
            }

            request.Comment = command.Comment;

            await _unitOfWork.CommitAsync();
        }

        private void PartialReject(TvRequestRow request, Dictionary<int, List<int>> episodesBySeason)
        {
            if (episodesBySeason == null)
            {
                return;
            }

            foreach (var season in request.TvRequestSeasons)
            {
                if (episodesBySeason.TryGetValue(season.SeasonIndex, out var requestEpisodes))
                {
                    if (requestEpisodes == null)
                    {
                        continue;
                    }

                    foreach (var episode in season.TvRequestEpisodes)
                    {
                        if (requestEpisodes.Contains(episode.EpisodeIndex))
                        {
                            RejectEpisode(episode);
                        }
                    }
                }
            }
        }

        private void RejectAll(TvRequestRow request)
        {
            foreach (var season in request.TvRequestSeasons)
            {
                foreach (var episode in season.TvRequestEpisodes)
                {
                    RejectEpisode(episode);
                }
            }
        }

        private void RejectEpisode(TvRequestEpisodeRow episode)
        {
            if (episode.RequestStatus != RequestStatuses.Completed)
            {
                episode.RequestStatus = RequestStatuses.Rejected;
            }
        }
    }
}