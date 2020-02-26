using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Enums;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace PlexRequests.ApiRequests.Issues.Commands
{
    public class UpdateIssueCommandHandler : IRequestHandler<UpdateIssueCommand, ValidationContext>
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
        
        public async Task<ValidationContext> Handle(UpdateIssueCommand command, CancellationToken cancellationToken)
        {
            var resultContext = new ValidationContext();

            ValidateCommand(command, resultContext);

            var issue = await _issueService.GetIssueById(command.Id);

            resultContext.AddErrorIf(() => issue == null, "Invalid IssueId", "No issue was found with the given Id");
            resultContext.AddErrorIf(() => issue?.IssueStatus == IssueStatuses.Resolved, "Issue already resolved", "The issue has already been resolved");

            if (!resultContext.IsSuccessful)
            {
                return resultContext;
            }

            issue.IssueStatus = command.Status;

            await _unitOfWork.CommitAsync();

            return resultContext;
        }

        private static void ValidateCommand(UpdateIssueCommand command, ValidationContext context)
        {
            context.AddErrorIf(() => command.Status == IssueStatuses.Resolved || string.IsNullOrEmpty(command.Outcome), 
                "Outcome required", "An outcome is required when resolving an issue");
            context.AddErrorIf(() => command.Status != IssueStatuses.Resolved && !string.IsNullOrWhiteSpace(command.Outcome),
                "Outcome should not be supplied", "An outcome can only be set when resolving an issue");
        }
    }
}