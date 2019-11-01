using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Services;
using PlexRequests.Plex;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;
using PlexRequests.TheMovieDb;
using PlexRequests.TheMovieDb.Models;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class CreateRequestCommandHandler : AsyncRequestHandler<CreateMovieRequestCommand>
    {
        private readonly ITheMovieDbService _theMovieDbService;
        private readonly IMovieRequestService _requestService;
        private readonly IPlexService _plexService;
        private readonly IClaimsPrincipalAccessor _claimsPrincipalAccessor;
        private readonly ILogger<CreateRequestCommandHandler> _logger;

        public CreateRequestCommandHandler(ITheMovieDbService theMovieDbService,
            IMovieRequestService requestService,
            IPlexService plexService,
            IClaimsPrincipalAccessor claimsPrincipalAccessor,
            ILogger<CreateRequestCommandHandler> logger)
        {
            _theMovieDbService = theMovieDbService;
            _requestService = requestService;
            _plexService = plexService;
            _claimsPrincipalAccessor = claimsPrincipalAccessor;
            _logger = logger;
        }

        protected override async Task Handle(CreateMovieRequestCommand request, CancellationToken cancellationToken)
        {
            var movieDetail = await GetMovieDetails(request.TheMovieDbId);

            var externalIds = await _theMovieDbService.GetMovieExternalIds(request.TheMovieDbId);

            await ValidateRequestNotDuplicate(request);

            await ValidateRequestedItemNotInPlex(request.TheMovieDbId, externalIds);

            await CreateRequest(request, movieDetail, externalIds);
        }

        private async Task<MovieDetails> GetMovieDetails(int theMovieDbId)
        {
            return await _theMovieDbService.GetMovieDetails(theMovieDbId);
        }

        private async Task CreateRequest(CreateMovieRequestCommand request, MovieDetails movieDetail, ExternalIds externalIds)
        {
            var newRequest = new MovieRequest
            {
                PrimaryAgent = new MediaAgent(AgentTypes.TheMovieDb, request.TheMovieDbId.ToString()),
                RequestedByUserId = _claimsPrincipalAccessor.UserId,
                RequestedByUserName = _claimsPrincipalAccessor.Username,
                Title = movieDetail.Title,
                AirDate = DateTime.Parse(movieDetail.Release_Date),
                ImagePath = movieDetail.Poster_Path,
                Created = DateTime.UtcNow
            };

            if (!string.IsNullOrEmpty(externalIds.Imdb_Id))
            {
                newRequest.AdditionalAgents = new List<MediaAgent>
                {
                    new MediaAgent(AgentTypes.Imdb, externalIds.Imdb_Id)
                };
            }

            await _requestService.Create(newRequest);
        }

        private async Task ValidateRequestNotDuplicate(CreateMovieRequestCommand request)
        {
            var existingRequest =
                await _requestService.GetExistingRequest(AgentTypes.TheMovieDb, request.TheMovieDbId.ToString());

            if (existingRequest != null)
            {
                _logger.LogDebug($"Request not created as existing request: {existingRequest.Id}");
                throw new PlexRequestException("Request not created", "The Movie has already been requested.");
            }
        }

        private async Task ValidateRequestedItemNotInPlex(int theMovieDbId, ExternalIds externalIds)
        {
            var plexMediaItem = await GetExistingPlexMediaItem(theMovieDbId, externalIds);

            if (plexMediaItem != null)
            {
                _logger.LogDebug($"Request not created as existing Plex item: {plexMediaItem.Id}");
                throw new PlexRequestException("Request not created", "The Movie is already available in Plex.");
            }
        }

        private async Task<PlexMediaItem> GetExistingPlexMediaItem(int theMovieDbId, ExternalIds externalIds)
        {
            var plexMediaItem = await _plexService.GetExistingMediaItemByAgent(PlexMediaTypes.Movie, AgentTypes.TheMovieDb, theMovieDbId.ToString());

            if (plexMediaItem == null && !string.IsNullOrEmpty(externalIds?.Imdb_Id))
            {
                plexMediaItem =
                    await _plexService.GetExistingMediaItemByAgent(PlexMediaTypes.Movie, AgentTypes.Imdb,
                        externalIds.Imdb_Id);
            }

            return plexMediaItem;
        }
    }
}