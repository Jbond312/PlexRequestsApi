using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Core.Auth;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace PlexRequests.Functions.Features.Requests.Commands
{
    public class DeleteMovieRequestCommandHandler : IRequestHandler<DeleteMovieRequestCommand, ValidationContext>
    {
        private readonly IMovieRequestService _requestService;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteMovieRequestCommandHandler(
            IMovieRequestService requestService,
            IUnitOfWork unitOfWork
            )
        {
            _requestService = requestService;
            _unitOfWork = unitOfWork;
        }
        
        public async Task<ValidationContext> Handle(DeleteMovieRequestCommand command, CancellationToken cancellationToken)
        {
            var resultContext = new ValidationContext();

            var request = await ValidateRequestExists(command, resultContext);

            ValidateUserCanDeleteRequest(request, resultContext, command.UserInfo);

            if (!resultContext.IsSuccessful)
            {
                return resultContext;
            }

            await _requestService.DeleteRequest(command.Id);

            await _unitOfWork.CommitAsync();

            return resultContext;
        }

        private void ValidateUserCanDeleteRequest(MovieRequestRow request, ValidationContext resultContext,
            UserInfo userInfo)
        {
            if (request != null && !request.UserId.Equals(userInfo.UserId))
            {
                resultContext.AddError("Unable to delete request", "Forbidden access to protected resource.");
            }
        }

        private async Task<MovieRequestRow> ValidateRequestExists(DeleteMovieRequestCommand command, ValidationContext resultContext)
        {
            var request = await _requestService.GetRequestById(command.Id);

            if (request == null)
            {
                resultContext.AddError("Invalid request id", "A request for the given id was not found.");
            }

            return request;
        }
    }
}