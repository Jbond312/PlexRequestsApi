using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Enums;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace PlexRequests.ApiRequests.Issues.Commands
{
    public class UpdateIssueCommandHandler : AsyncRequestHandler<UpdateIssueCommand>
    {
        private readonly IIssueService _issueService;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateIssueCommandHandler(
            IIssueService issueService,
            IUnitOfWork unitOfWork
            )
        {
            _issueService = issueService;
            _unitOfWork = unitOfWork;
        }

        protected override async Task Handle(UpdateIssueCommand command, CancellationToken cancellationToken)
        {
            ValidateCommand(command);

            var issue = await _issueService.GetIssueById(command.Id);

            if (issue == null)
            {
                throw new PlexRequestException("Issue not updated", "No issue was found with the given Id", HttpStatusCode.NotFound);
            }

            if (issue.IssueStatus == IssueStatuses.Resolved)
            {
                throw new PlexRequestException("Issue not updated", "The issue has already been resolved");
            }

            issue.IssueStatus = command.Status;

            await _unitOfWork.CommitAsync();
        }

        private void ValidateCommand(UpdateIssueCommand command)
        {
            if (command.Status == IssueStatuses.Resolved && string.IsNullOrWhiteSpace(command.Outcome))
            {
                throw new PlexRequestException("Issue not updated", "An outcome is required when resolving an issue");
            }

            if (command.Status != IssueStatuses.Resolved && !string.IsNullOrWhiteSpace(command.Outcome))
            {
                throw new PlexRequestException("Issue not updated", "An outcome can only be set when resolving an issue");
            }
        }
    }
}