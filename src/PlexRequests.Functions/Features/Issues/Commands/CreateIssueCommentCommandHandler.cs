using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace PlexRequests.Functions.Features.Issues.Commands
{
    public class CreateIssueCommentCommandHandler : IRequestHandler<CreateIssueCommentCommand, ValidationContext>
    {
        private readonly IIssueService _issueService;
        private readonly IUnitOfWork _unitOfWork;

        public CreateIssueCommentCommandHandler(
            IIssueService issueService,
            IUnitOfWork unitOfWork)
        {
            _issueService = issueService;
            _unitOfWork = unitOfWork;
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
                UserId = command.UserInfo.UserId,
                Comment = command.Comment
            };

            issue.IssueComments.Add(issueComment);

            await _unitOfWork.CommitAsync();

            return resultContext;
        }
    }
}