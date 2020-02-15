using System.Linq;
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
    public class DeleteTvRequestCommandHandler : AsyncRequestHandler<DeleteTvRequestCommand>
    {
        private readonly ITvRequestService _requestService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsPrincipalAccessor _claimsUserAccessor;

        public DeleteTvRequestCommandHandler(
            ITvRequestService requestService,
            IUnitOfWork unitOfWork,
            IClaimsPrincipalAccessor claimsUserAccessor
            )
        {
            _requestService = requestService;
            _unitOfWork = unitOfWork;
            _claimsUserAccessor = claimsUserAccessor;
        }
        
        protected override async Task Handle(DeleteTvRequestCommand command, CancellationToken cancellationToken)
        {
            var request = await ValidateRequestExists(command);

            var requestUser = request.TvRequestUsers.FirstOrDefault(x => x.UserId == _claimsUserAccessor.UserId);

            if(requestUser == null)
            {
                return;
            }

            ValidateUserCanDeleteRequest(requestUser);

            request.TvRequestUsers.Remove(requestUser);

            if(!request.TvRequestUsers.Any())
            {
                await _requestService.DeleteRequest(command.Id);
            }

            await _unitOfWork.CommitAsync();
        }

        private void ValidateUserCanDeleteRequest(TvRequestUserRow tvRequestUser)
        {
            if (!tvRequestUser.UserId.Equals(_claimsUserAccessor.UserId))
            {
                throw new PlexRequestException("Unable to delete request", "Forbidden access to protected resource.",
                    HttpStatusCode.Forbidden);
            }
        }

        private async Task<TvRequestRow> ValidateRequestExists(DeleteTvRequestCommand command)
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