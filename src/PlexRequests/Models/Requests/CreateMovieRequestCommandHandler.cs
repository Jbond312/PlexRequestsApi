using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using PlexRequests.Core;
using PlexRequests.Helpers;
using PlexRequests.Plex;
using PlexRequests.Store.Enums;
using PlexRequests.Store.Models;
using PlexRequests.TheMovieDb;
using PlexRequests.TheMovieDb.Models;

namespace PlexRequests.Models.Requests
{
    public class CreateRequestCommandHandler : AsyncRequestHandler<CreateMovieRequestCommand>
    {
        private readonly ITheMovieDbApi _theMovieDbApi;
        private readonly IRequestService _requestService;
        private readonly IPlexService _plexService;
        private readonly IClaimsPrincipalAccessor _claimsPrincipalAccessor;
        private readonly ILogger<CreateRequestCommandHandler> _logger;

        public CreateRequestCommandHandler(ITheMovieDbApi theMovieDbApi,
            IRequestService requestService,
            IPlexService plexService,
            IClaimsPrincipalAccessor claimsPrincipalAccessor,
            ILogger<CreateRequestCommandHandler> logger)
        {
            _theMovieDbApi = theMovieDbApi;
            _requestService = requestService;
            _plexService = plexService;
            _claimsPrincipalAccessor = claimsPrincipalAccessor;
            _logger = logger;
        }
        
        protected override async Task Handle(CreateMovieRequestCommand request, CancellationToken cancellationToken)
        {
            var movieDetail = await GetMovieDetails(request.TheMovieDbId);
            
            await ValidateRequestNotDuplicate(request);
            
            await ValidateRequestedItemNotInPlex(request);

            await CreateRequest(request, movieDetail);
        }

        private async Task<MovieDetails> GetMovieDetails(int theMovieDbId)
        {
            return await _theMovieDbApi.GetMovieDetails(theMovieDbId);
        }

        private async Task CreateRequest(CreateMovieRequestCommand request, MovieDetails movieDetail)
        {
            var newRequest = new Request
            {
                AgentType = AgentTypes.TheMovieDb,
                AgentSourceId = request.TheMovieDbId.ToString(),
                MediaType = PlexMediaTypes.Movie,
                RequestedByUserId = _claimsPrincipalAccessor.UserId,
                RequestedByUserName = _claimsPrincipalAccessor.Username,
                Title = movieDetail.Title,
                AirDate = DateTime.Parse(movieDetail.Release_Date),
                ImagePath = movieDetail.Poster_Path,
                Created = DateTime.UtcNow
            };

            await _requestService.Create(newRequest);
        }

        private async Task ValidateRequestNotDuplicate(CreateMovieRequestCommand request)
        {
            var existingRequest =
                await _requestService.GetExistingMovieRequest(AgentTypes.TheMovieDb, request.TheMovieDbId.ToString());

            if (existingRequest != null)
            {
                _logger.LogDebug($"Request not created as existing request: {existingRequest.Id}");
                throw new PlexRequestException("Request not created", "The Movie has already been requested.");
            }
        }

        private async Task ValidateRequestedItemNotInPlex(CreateMovieRequestCommand request)
        {
            var externalIds = await _theMovieDbApi.GetMovieExternalIds(request.TheMovieDbId);
            
            var plexMediaItem = await GetExistingPlexMediaItem(request.TheMovieDbId, externalIds);

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