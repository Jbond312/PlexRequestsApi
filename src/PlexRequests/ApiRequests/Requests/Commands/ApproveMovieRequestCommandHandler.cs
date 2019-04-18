using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Services;
using PlexRequests.Repository.Enums;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class ApproveMovieRequestCommandHandler : AsyncRequestHandler<ApproveMovieRequestCommand>
    {
        private readonly IRequestService _requestService;

        public ApproveMovieRequestCommandHandler(
            IRequestService requestService
            )
        {
            _requestService = requestService;
        }
        
        protected override async Task Handle(ApproveMovieRequestCommand command, CancellationToken cancellationToken)
        {
            var request = await _requestService.GetRequestById(command.RequestId);

            if (request == null)
            {
                throw new PlexRequestException("Invalid request", "No request was found with the given Id", HttpStatusCode.NotFound);
            }

            request.Status = RequestStatuses.Approved;
            
            await _requestService.Update(request);
        }
    }
}