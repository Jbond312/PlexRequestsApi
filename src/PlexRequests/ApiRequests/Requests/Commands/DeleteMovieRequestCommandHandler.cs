using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class DeleteMovieRequestCommandHandler : AsyncRequestHandler<DeleteMovieRequestCommand>
    {
        private readonly IMovieRequestService _requestService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsPrincipalAccessor _claimsUserAccessor;

        public DeleteMovieRequestCommandHandler(
            IMovieRequestService requestService,
            IUnitOfWork unitOfWork,
            IClaimsPrincipalAccessor claimsUserAccessor
            )
        {
            _requestService = requestService;
            _unitOfWork = unitOfWork;
            _claimsUserAccessor = claimsUserAccessor;
        }
        
        protected override async Task Handle(DeleteMovieRequestCommand command, CancellationToken cancellationToken)
        {
            var request = await ValidateRequestExists(command);

            ValidateUserCanDeleteRequest(request);

            await _requestService.DeleteRequest(command.Id);

            await _unitOfWork.CommitAsync();
        }

        private void ValidateUserCanDeleteRequest(MovieRequestRow request)
        {
            if (!request.UserId.Equals(_claimsUserAccessor.UserId))
            {
                throw new PlexRequestException("Unable to delete request", "Forbidden access to protected resource.",
                    HttpStatusCode.Forbidden);
            }
        }

        private async Task<MovieRequestRow> ValidateRequestExists(DeleteMovieRequestCommand command)
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