using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Services;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;
using PlexRequests.TheMovieDb;
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace PlexRequests.ApiRequests.Issues.Commands
{
    public class CreateIssueCommandHandler : AsyncRequestHandler<CreateIssueCommand>
    {
        private readonly IIssueService _issueService;
        private readonly ITheMovieDbApi _theMovieDbApi;
        private readonly IClaimsPrincipalAccessor _claimsPrincipalAccessor;

        public CreateIssueCommandHandler(
            IIssueService issueService,
            ITheMovieDbApi theMovieDbApi,
            IClaimsPrincipalAccessor claimsPrincipalAccessor)
        {
            _issueService = issueService;
            _theMovieDbApi = theMovieDbApi;
            _claimsPrincipalAccessor = claimsPrincipalAccessor;
        }

        protected override async Task Handle(CreateIssueCommand command, CancellationToken cancellationToken)
        {
            ValidateCommand(command);

            var mediaDetails = await GetMediaDetails(command.TheMovieDbId, command.MediaType);

            await CreateIssue(command, mediaDetails);
        }

        private async Task CreateIssue(CreateIssueCommand command, (MediaAgent mediaAgent, string name, string imagePath, DateTime airDate) mediaDetails)
        {
            var issue = new Issue
            {
                MediaItemName = mediaDetails.name,
                Title = command.Title,
                Description = command.Description,
                MediaType = command.MediaType,
                MediaAgent = mediaDetails.mediaAgent,
                Status = IssueStatuses.Pending,
                RequestedByUserId = _claimsPrincipalAccessor.UserId,
                RequestedByUserName = _claimsPrincipalAccessor.Username,
                ImagePath = mediaDetails.imagePath,
                AirDate = mediaDetails.airDate,
                Created = DateTime.UtcNow,
                Comments = new List<IssueComment>()
            };

            await _issueService.Create(issue);
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

        private async Task<(MediaAgent mediaAgent, string name, string imagePath, DateTime airDate)> GetMediaDetails(int theMovieDbId, PlexMediaTypes mediaType)
        {
            string name;
            string imagePath;
            DateTime airDate;
            if (mediaType == PlexMediaTypes.Show)
            {
                var show = await _theMovieDbApi.GetTvDetails(theMovieDbId);
                name = show.Name;
                imagePath = show.Poster_Path;
                airDate = DateTime.Parse(show.First_Air_Date);
            }
            else
            {
                var movie = await _theMovieDbApi.GetMovieDetails(theMovieDbId);
                name = movie.Title;
                imagePath = movie.Poster_Path;
                airDate = DateTime.Parse(movie.Release_Date);
            }

            var mediaAgent = new MediaAgent(AgentTypes.TheMovieDb, theMovieDbId.ToString());

            return (mediaAgent, name, imagePath, airDate);
        }
    }
}