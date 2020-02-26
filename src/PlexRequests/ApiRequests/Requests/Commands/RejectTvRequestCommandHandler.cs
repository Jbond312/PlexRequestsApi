using MediatR;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class RejectTvRequestCommandHandler : IRequestHandler<RejectTvRequestCommand, ValidationContext>
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

        public async Task<ValidationContext> Handle(RejectTvRequestCommand command, CancellationToken cancellationToken)
        {
            var result = new ValidationContext();

            if (string.IsNullOrWhiteSpace(command.Comment))
            {
                result.AddError("Invalid request", "A comment must be specified when rejecting a request");
            }

            var request = await _requestService.GetRequestById(command.RequestId);

            if (request == null)
            {
                result.AddError("Invalid request", "No request was found with the given Id");
            }

            if (request?.RequestStatus == RequestStatuses.Completed)
            {
                result.AddError("Invalid request", "Request has already been completed");
            }

            if (!result.IsSuccessful)
            {
                return result;
            }

            // ReSharper disable once PossibleNullReferenceException
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

            return result;
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