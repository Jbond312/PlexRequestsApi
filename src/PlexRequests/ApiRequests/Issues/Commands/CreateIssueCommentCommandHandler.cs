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
    public class CreateIssueCommentCommandHandler : IRequestHandler<CreateIssueCommentCommand, ValidationContext>
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

        public async Task<ValidationContext> Handle(CreateIssueCommentCommand command, CancellationToken cancellationToken)
        {
            var resultContext = new ValidationContext();

            resultContext.AddErrorIf(() => string.IsNullOrEmpty(command.Comment), "Invalid Comment", "A comment must be specified");

            var issue = await _issueService.GetIssueById(command.Id);

            resultContext.AddErrorIf(() => issue == null, "Comment not created", "An issue could not be found with the given id");

            if (!resultContext.IsSuccessful)
            {
                return resultContext;
            }

            var issueComment = new IssueCommentRow
            {
                UserId = _claimsPrincipalAccessor.UserId,
                Comment = command.Comment
            };

            issue.IssueComments.Add(issueComment);

            await _unitOfWork.CommitAsync();

            return resultContext;
        }
    }
}