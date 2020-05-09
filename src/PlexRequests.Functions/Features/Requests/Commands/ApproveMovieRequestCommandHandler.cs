using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.Functions.Features.Requests.Commands
{
    public class ApproveMovieRequestCommandHandler : IRequestHandler<ApproveMovieRequestCommand, ValidationContext>
    {
        private readonly IMovieRequestService _requestService;
        private readonly IUnitOfWork _unitOfWork;

        public ApproveMovieRequestCommandHandler(
            IMovieRequestService requestService,
            IUnitOfWork unitOfWork
            )
        {
            _requestService = requestService;
            _unitOfWork = unitOfWork;
        } 
        
        public async Task<ValidationContext> Handle(ApproveMovieRequestCommand command, CancellationToken cancellationToken)
        {
            var result = new ValidationContext();

            var request = await _requestService.GetRequestById(command.RequestId);

            result.AddErrorIf(() => request == null, "Invalid request", "No request was found with the given Id");
            result.AddErrorIf(() => request?.RequestStatus == RequestStatuses.Completed, "Invalid request", "Request has already been completed");

            if (!result.IsSuccessful)
            {
                return result;
            }

            request.RequestStatus = RequestStatuses.Approved;

            await _unitOfWork.CommitAsync();

            return result;
        }
    }
}