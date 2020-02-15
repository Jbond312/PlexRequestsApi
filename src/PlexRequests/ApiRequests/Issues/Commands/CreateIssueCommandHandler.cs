using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using PlexRequests.Plex;
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace PlexRequests.ApiRequests.Issues.Commands
{
    public class CreateIssueCommandHandler : AsyncRequestHandler<CreateIssueCommand>
    {
        private readonly IIssueService _issueService;
        private readonly IPlexService _plexService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsPrincipalAccessor _claimsPrincipalAccessor;

        public CreateIssueCommandHandler(
            IIssueService issueService,
            IPlexService plexService,
            IUnitOfWork unitOfWork,
            IClaimsPrincipalAccessor claimsPrincipalAccessor)
        {
            _issueService = issueService;
            _plexService = plexService;
            _unitOfWork = unitOfWork;
            _claimsPrincipalAccessor = claimsPrincipalAccessor;
        }

        protected override async Task Handle(CreateIssueCommand command, CancellationToken cancellationToken)
        {
            ValidateCommand(command);

            var plexMediaItem = await GetPlexMediaItem(command.TheMovieDbId, command.MediaType);

            CreateIssue(command, plexMediaItem);

            await _unitOfWork.CommitAsync();
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
                UserId = _claimsPrincipalAccessor.UserId,
            };

            _issueService.Add(issue);
        }

        private void ValidateCommand(CreateIssueCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Title))
            {
                throw new PlexRequestException("Issue not created", "'Title' must be specified");
            }

            if (string.IsNullOrWhiteSpace(command.Description))
            {
                throw new PlexRequestException("Issue not created", "'Description' must be specified");
            }
        }
    }
}