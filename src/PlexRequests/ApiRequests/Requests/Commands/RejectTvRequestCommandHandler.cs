using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Services;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class RejectTvRequestCommandHandler : AsyncRequestHandler<RejectTvRequestCommand>
    {
        private readonly IRequestService _requestService;

        public RejectTvRequestCommandHandler(IRequestService requestService)
        {
            _requestService = requestService;
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

            if (request.Status == RequestStatuses.Completed)
            {
                throw new PlexRequestException("Invalid request", "Request has already been completed");
            }

            if (command.RejectAll)
            {
                RejectAll(request);
            }
            else
            {
                PartialReject(request, command.EpisodesBySeason);
            }

            request.Status = _requestService.CalculateAggregatedStatus(request);
            request.Comment = command.Comment;

            await _requestService.Update(request);
        }

        private void PartialReject(Request request, Dictionary<int, List<int>> episodesBySeason)
        {
            if (episodesBySeason == null)
            {
                return;
            }

            foreach (var season in request.Seasons)
            {
                if (episodesBySeason.TryGetValue(season.Index, out var requestEpisodes))
                {
                    if (requestEpisodes == null)
                    {
                        continue;
                    }

                    foreach (var episode in season.Episodes)
                    {
                        if (requestEpisodes.Contains(episode.Index))
                        {
                            RejectEpisode(episode);
                        }
                    }
                }
            }
        }

        private void RejectAll(Request request)
        {
            foreach (var season in request.Seasons)
            {
                foreach (var episode in season.Episodes)
                {
                    RejectEpisode(episode);
                }
            }
        }

        private void RejectEpisode(RequestEpisode episode)
        {
            if (episode.Status != RequestStatuses.Completed)
            {
                episode.Status = RequestStatuses.Rejected;
            }
        }
    }
}