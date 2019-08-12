using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Services;
using PlexRequests.Repository.Enums;
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace PlexRequests.ApiRequests.Issues.Commands
{
    public class UpdateIssueCommandHandler : AsyncRequestHandler<UpdateIssueCommand>
    {
        private readonly IIssueService _issueService;

        public UpdateIssueCommandHandler(IIssueService issueService)
        {
            _issueService = issueService;
        }

        protected override async Task Handle(UpdateIssueCommand command, CancellationToken cancellationToken)
        {
            ValidateCommand(command);

            var issue = await _issueService.GetIssueById(command.Id);

            if (issue == null)
            {
                throw new PlexRequestException("Issue not updated", "No issue was found with the given Id", HttpStatusCode.NotFound);
            }

            if (issue.Status == IssueStatuses.Resolved)
            {
                throw new PlexRequestException("Issue not updated", "The issue has already been resolved");
            }

            issue.Status = command.Status;
            issue.Resolution = command.Resolution;

            await _issueService.Update(issue);
        }

        private void ValidateCommand(UpdateIssueCommand command)
        {
            if (command.Status == IssueStatuses.Resolved && string.IsNullOrWhiteSpace(command.Resolution))
            {
                throw new PlexRequestException("Issue not updated", "A resolution is required when resolving an issue");
            }

            if (command.Status != IssueStatuses.Resolved && !string.IsNullOrWhiteSpace(command.Resolution))
            {
                throw new PlexRequestException("Issue not updated", "A resolution can only be set when resolving an issue");
            }
        }
    }
}