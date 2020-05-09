using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using PlexRequests.Plex;
using PlexRequests.TheMovieDb;
using PlexRequests.TheMovieDb.Models;

namespace PlexRequests.Functions.Features.Requests.Commands
{
    public class CreateRequestCommandHandler : IRequestHandler<CreateMovieRequestCommand, ValidationContext>
    {
        private readonly ITheMovieDbService _theMovieDbService;
        private readonly IMovieRequestService _requestService;
        private readonly IPlexService _plexService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsPrincipalAccessor _claimsPrincipalAccessor;
        private readonly ILogger<CreateRequestCommandHandler> _logger;

        public CreateRequestCommandHandler(
            ITheMovieDbService theMovieDbService,
            IMovieRequestService requestService,
            IPlexService plexService,
            IUnitOfWork unitOfWork,
            IClaimsPrincipalAccessor claimsPrincipalAccessor,
            ILogger<CreateRequestCommandHandler> logger)
        {
            _theMovieDbService = theMovieDbService;
            _requestService = requestService;
            _plexService = plexService;
            _unitOfWork = unitOfWork;
            _claimsPrincipalAccessor = claimsPrincipalAccessor;
            _logger = logger;
        }

        public async Task<ValidationContext> Handle(CreateMovieRequestCommand request, CancellationToken cancellationToken)
        {
            var resultContext = new ValidationContext();

            var movieDetail = await GetMovieDetails(request.TheMovieDbId);

            var externalIds = await _theMovieDbService.GetMovieExternalIds(request.TheMovieDbId);

            await ValidateRequestNotDuplicate(request, resultContext);

            await ValidateRequestedItemNotInPlex(request.TheMovieDbId, externalIds, resultContext);

            if (!resultContext.IsSuccessful)
            {
                return resultContext;
            }

            await CreateRequest(request, movieDetail, externalIds);

            await _unitOfWork.CommitAsync();

            return resultContext;
        }

        private async Task<MovieDetails> GetMovieDetails(int theMovieDbId)
        {
            return await _theMovieDbService.GetMovieDetails(theMovieDbId);
        }

        private async Task CreateRequest(CreateMovieRequestCommand request, MovieDetails movieDetail, ExternalIds externalIds)
        {
            var newRequest = new MovieRequestRow
            {
                UserId = _claimsPrincipalAccessor.UserId,
                Title = movieDetail.Title,
                AirDateUtc = DateTime.Parse(movieDetail.Release_Date),
                ImagePath = movieDetail.Poster_Path,
                TheMovieDbId = request.TheMovieDbId
            };

            newRequest.MovieRequestAgents.Add(new MovieRequestAgentRow(AgentTypes.TheMovieDb, request.TheMovieDbId.ToString()));

            if (!string.IsNullOrEmpty(externalIds.Imdb_Id))
            {
                newRequest.MovieRequestAgents.Add(new MovieRequestAgentRow(AgentTypes.Imdb, externalIds.Imdb_Id));
            }

            await _requestService.Add(newRequest);
        }

        private async Task ValidateRequestNotDuplicate(CreateMovieRequestCommand request, ValidationContext resultContext)
        {
            var existingRequest =
                await _requestService.GetExistingRequest(AgentTypes.TheMovieDb, request.TheMovieDbId.ToString());

            if (existingRequest != null)
            {
                resultContext.AddError("Request not created", "The Movie has already been requested.");
                _logger.LogDebug($"Request not created as existing request: {existingRequest.MovieRequestId}");
            }
        }

        private async Task ValidateRequestedItemNotInPlex(int theMovieDbId, ExternalIds externalIds, ValidationContext resultContext)
        {
            var plexMediaItem = await GetExistingPlexMediaItem(theMovieDbId, externalIds);

            if (plexMediaItem != null)
            {
                _logger.LogDebug($"Request not created as existing Plex item: {plexMediaItem.PlexMediaItemId}");
                resultContext.AddError("Request not created", "The Movie is already available in Plex.");
            }
        }

        private async Task<PlexMediaItemRow> GetExistingPlexMediaItem(int theMovieDbId, ExternalIds externalIds)
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