using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class RejectMovieRequestCommandHandler : AsyncRequestHandler<RejectMovieRequestCommand>
    {
        private readonly IMovieRequestService _requestService;
        private readonly IUnitOfWork _unitOfWork;

        public RejectMovieRequestCommandHandler(
        IMovieRequestService requestService,
        IUnitOfWork unitOfWork
        )
        {
            _requestService = requestService;
            _unitOfWork = unitOfWork;
        }

        protected override async Task Handle(RejectMovieRequestCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(command.Comment))
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

            request.RequestStatus = RequestStatuses.Rejected;
            request.Comment = command.Comment;

            await _unitOfWork.CommitAsync();
        }
    }
}