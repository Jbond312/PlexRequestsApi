using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using PlexRequests.Plex;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace PlexRequests.Functions.Features.Issues.Commands
{
    public class CreateIssueCommandHandler : IRequestHandler<CreateIssueCommand, ValidationContext>
    {
        private readonly IIssueService _issueService;
        private readonly IPlexService _plexService;
        private readonly IUnitOfWork _unitOfWork;

        public CreateIssueCommandHandler(
            IIssueService issueService,
            IPlexService plexService,
            IUnitOfWork unitOfWork)
        {
            _issueService = issueService;
            _plexService = plexService;
            _unitOfWork = unitOfWork;
        }

        public async Task<ValidationContext> Handle(CreateIssueCommand command, CancellationToken cancellationToken)
        {
            var result = new ValidationContext();

            ValidateCommand(command, result);

            var plexMediaItem = await GetPlexMediaItem(command.TheMovieDbId, command.MediaType);

            if (!result.IsSuccessful)
            {
                return result;
            }

            CreateIssue(command, plexMediaItem);

            await _unitOfWork.CommitAsync();

            return result;
        }

        private async Task<PlexMediaItemRow> GetPlexMediaItem(int theMovieDbId, PlexMediaTypes mediaType)
        {
            var plexMediaItem = await _plexService.GetExistingMediaItemByAgent(mediaType, AgentTypes.TheMovieDb, theMovieDbId.ToString());

            return plexMediaItem;
        }

        private void CreateIssue(CreateIssueCommand command, PlexMediaItemRow plexMediaItem)
        {
            var issue = new IssueRow
            {
                PlexMediaItem = plexMediaItem,
                Title = command.Title,
                Description = command.Description,
                IssueStatus = IssueStatuses.Pending,
                UserId = command.UserInfo.UserId,
            };

            _issueService.Add(issue);
        }

        private void ValidateCommand(CreateIssueCommand command, ValidationContext result)
        {
            result.AddErrorIf(() => string.IsNullOrWhiteSpace(command.Title), "Issue not created", $"'{nameof(command.Title)}' must be specified");
            result.AddErrorIf(() => string.IsNullOrWhiteSpace(command.Description), "Issue not created", $"'{nameof(command.Description)}' must be specified");
        }
    }
}