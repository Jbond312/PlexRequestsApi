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

namespace PlexRequests.ApiRequests.Issues.Commands
{
    public class CreateIssueCommentCommandHandler : AsyncRequestHandler<CreateIssueCommentCommand>
    {
        private readonly IIssueService _issueService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsPrincipalAccessor _claimsPrincipalAccessor;

        public CreateIssueCommentCommandHandler(
            IIssueService issueService,
            IUnitOfWork unitOfWork,
            IClaimsPrincipalAccessor claimsPrincipalAccessor)
        {
            _issueService = issueService;
            _unitOfWork = unitOfWork;
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

            var issueComment = new IssueCommentRow
            {
                UserId = _claimsPrincipalAccessor.UserId,
                Comment = command.Comment
            };

            issue.IssueComments.Add(issueComment);

            await _unitOfWork.CommitAsync();
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