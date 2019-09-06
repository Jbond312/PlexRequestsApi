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
    public class ApproveTvRequestCommandHandler : AsyncRequestHandler<ApproveTvRequestCommand>
    {
        private readonly ITvRequestService _requestService;

        public ApproveTvRequestCommandHandler(
            ITvRequestService requestService
            )
        {
            _requestService = requestService;
        }

        protected override async Task Handle(ApproveTvRequestCommand command, CancellationToken cancellationToken)
        {
            var request = await _requestService.GetRequestById(command.RequestId);

            if (request == null)
            {
                throw new PlexRequestException("Invalid request", "No request was found with the given Id", HttpStatusCode.NotFound);
            }

            if (request.Status == RequestStatuses.Completed)
            {
                throw new PlexRequestException("Invalid request", "Request has already been completed");
            }

            if (request.Track)
            {
                request.Status = RequestStatuses.Approved;
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

                request.Status = _requestService.CalculateAggregatedStatus(request);
            }

            await _requestService.Update(request);
        }

        private static void ApproveEpisodes(TvRequest request, IReadOnlyDictionary<int, List<int>> commandEpisodesBySeason)
        {
            foreach (var season in request.Seasons)
            {
                if (!commandEpisodesBySeason.TryGetValue(season.Index, out var commandEpisodes))
                {
                    continue;
                }

                foreach (var episode in season.Episodes)
                {
                    if (!commandEpisodes.Contains(episode.Index))
                    {
                        continue;
                    }

                    ApproveEpisode(episode);
                }
            }
        }

        private static void ApproveAllEpisodes(TvRequest request)
        {
            foreach (var season in request.Seasons)
            {
                foreach (var episode in season.Episodes)
                {
                    ApproveEpisode(episode);
                }
            }
        }

        private static void ApproveEpisode(RequestEpisode episode)
        {
            if (episode.Status != RequestStatuses.Completed)
            {
                episode.Status = RequestStatuses.Approved;
            }
        }
    }
}