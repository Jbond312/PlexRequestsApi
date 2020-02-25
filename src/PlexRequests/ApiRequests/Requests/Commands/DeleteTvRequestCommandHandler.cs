using MediatR;
using Microsoft.Extensions.Logging;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class DeleteTvRequestCommandHandler : IRequestHandler<DeleteTvRequestCommand, ValidationContext>
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
        
        public async Task<ValidationContext> Handle(DeleteTvRequestCommand command, CancellationToken cancellationToken)
        {
            var result = new ValidationContext();

            var request = await ValidateRequestExists(command, result);

            if (!result.IsSuccessful)
            {
                return result;
            }

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

            return result;
        }

        private async Task<TvRequestRow> ValidateRequestExists(DeleteTvRequestCommand command, ValidationContext result)
        {
            var request = await _requestService.GetRequestById(command.Id);

            if (request == null)
            {
                result.AddError("Invalid request id", "A request for the given id was not found.");
            }

            return request;
        }
    }
}