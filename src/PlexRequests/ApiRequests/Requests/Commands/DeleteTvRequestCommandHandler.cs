using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<DeleteTvRequestCommandHandler> _logger;

        public DeleteTvRequestCommandHandler(
            ITvRequestService requestService,
            IUnitOfWork unitOfWork,
            IClaimsPrincipalAccessor claimsUserAccessor,
            ILogger<DeleteTvRequestCommandHandler> logger
            )
        {
            _requestService = requestService;
            _unitOfWork = unitOfWork;
            _claimsUserAccessor = claimsUserAccessor;
            _logger = logger;
        }
        
        protected override async Task Handle(DeleteTvRequestCommand command, CancellationToken cancellationToken)
        {
            var request = await ValidateRequestExists(command);

            var requestUser = request.TvRequestUsers.FirstOrDefault(x => x.UserId == _claimsUserAccessor.UserId);

            if (requestUser != null)
            {
                request.TvRequestUsers.Remove(requestUser);
            }
            else
            {
                _logger.LogDebug($"TVRequest for user [{_claimsUserAccessor.UserId}] was not deleted as no matching requests were found");
            }

            if(!request.TvRequestUsers.Any())
            {
                _logger.LogDebug("Deleting entire TV request as no requests remain for any users");
                await _requestService.DeleteRequest(command.Id);
            }

            await _unitOfWork.CommitAsync();
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