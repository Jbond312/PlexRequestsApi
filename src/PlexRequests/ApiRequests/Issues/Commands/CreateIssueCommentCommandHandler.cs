using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Services;
using PlexRequests.Repository.Models;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class CreateIssueCommentCommandHandler : AsyncRequestHandler<CreateIssueCommentCommand>
    {
        private readonly IIssueService _issueService;
        private readonly IClaimsPrincipalAccessor _claimsPrincipalAccessor;

        public CreateIssueCommentCommandHandler(
            IIssueService issueService,
            IClaimsPrincipalAccessor claimsPrincipalAccessor)
        {
            _issueService = issueService;
            _claimsPrincipalAccessor = claimsPrincipalAccessor;
        }

        protected override async Task Handle(CreateIssueCommentCommand command, CancellationToken cancellationToken)
        {
            ValidateCommand(command);

            var issue = await _issueService.GetIssueById(command.Id);

            if (issue == null)
            {
                throw new PlexRequestException("Comment not created", "An issue could not be found with the given Id", HttpStatusCode.NotFound);
            }

            var issueComment = new IssueComment
            {
                UserId = _claimsPrincipalAccessor.UserId,
                UserName = _claimsPrincipalAccessor.Username,
                Comment = command.Comment,
                Created = DateTime.UtcNow
            };

            issue.Comments.Add(issueComment);

            await _issueService.Update(issue);
        }

        private void ValidateCommand(CreateIssueCommentCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Comment))
            {
                throw new PlexRequestException("Comment not created", "A comment must be specified");
            }
        }
    }
}