using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Core;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Services;
using PlexRequests.Repository.Models;

namespace PlexRequests.Models.Requests
{
    public class DeleteRequestCommandHandler : AsyncRequestHandler<DeleteRequestCommand>
    {
        private readonly IRequestService _requestService;
        private readonly IClaimsPrincipalAccessor _claimsUserAccessor;

        public DeleteRequestCommandHandler(
            IRequestService requestService,
            IClaimsPrincipalAccessor claimsUserAccessor
            )
        {
            _requestService = requestService;
            _claimsUserAccessor = claimsUserAccessor;
        }
        
        protected override async Task Handle(DeleteRequestCommand command, CancellationToken cancellationToken)
        {
            var request = await ValidateRequestExists(command);

            ValidateUserCanDeleteRequest(request);

            await _requestService.DeleteRequest(command.Id);
        }

        private void ValidateUserCanDeleteRequest(Request request)
        {
            if (!request.RequestedByUserId.Equals(_claimsUserAccessor.UserId))
            {
                throw new PlexRequestException("Unable to delete request", "Forbidden access to protected resource.",
                    HttpStatusCode.Forbidden);
            }
        }

        private async Task<Request> ValidateRequestExists(DeleteRequestCommand command)
        {
            var request = await _requestService.GetRequestById(command.Id);

            if (request == null)
            {
                throw new PlexRequestException("Invalid request id", "A request for the given id was not found.",
                    HttpStatusCode.NotFound);
            }

            return request;
        }
    }
}