using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.Functions.Features.Requests.Commands
{
    public class RejectMovieRequestCommandHandler : IRequestHandler<RejectMovieRequestCommand, ValidationContext>
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

        public async Task<ValidationContext> Handle(RejectMovieRequestCommand command, CancellationToken cancellationToken)
        {
            var result = new ValidationContext();

            if (string.IsNullOrEmpty(command.Comment))
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
            request.RequestStatus = RequestStatuses.Rejected;
            request.Comment = command.Comment;

            await _unitOfWork.CommitAsync();

            return result;
        }
    }
}