using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using PlexRequests.Core.Auth;
using PlexRequests.Functions.AccessTokens;
using PlexRequests.Functions.Features.Plex.Commands;
using PlexRequests.Functions.Features.Plex.Queries;

namespace PlexRequests.Functions.HttpTriggers
{
    public class PlexHttpTrigger
    {
        private readonly IMediator _mediator;
        private readonly IRequestValidator _requestValidator;
        private readonly IAccessTokenProvider _accessTokenProvider;

        public PlexHttpTrigger(
            IMediator mediator,
            IRequestValidator requestValidator,
            IAccessTokenProvider accessTokenProvider
        )
        {
            _mediator = mediator;
            _requestValidator = requestValidator;
            _accessTokenProvider = accessTokenProvider;
        }

        [FunctionName("SyncUsers")]
        public async Task<IActionResult> SyncUsers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "plex/users/sync")]
            HttpRequest req)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req, PlexRequestRoles.Admin);

            if (accessResult.Status == AccessTokenStatus.InsufficientPermissions)
            {
                return new ForbidResult();
            }

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            var resultContext = await _mediator.Send(new SyncUsersCommand());

            return resultContext.ToResultIfValid<NoContentResult>();
        }

        [FunctionName("GetLibraries")]
        public async Task<IActionResult> GetLibraries(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "plex/libraries")]
            HttpRequest req)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req, PlexRequestRoles.Admin);

            if (accessResult.Status == AccessTokenStatus.InsufficientPermissions)
            {
                return new ForbidResult();
            }

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            var query = new GetManyPlexServerLibraryQuery();
            var resultContext = await _mediator.Send(query);

            return resultContext.ToOkIfValidResult();
        }

        [FunctionName("UpdateLibrary")]
        public async Task<IActionResult> UpdateLibrary(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "plex/libraries/{key}")]
            HttpRequest req, string key)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req, PlexRequestRoles.Admin);

            if (accessResult.Status == AccessTokenStatus.InsufficientPermissions)
            {
                return new ForbidResult();
            }

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            var command = await req.DeserializeAndValidateRequest<UpdatePlexServerLibraryCommand>();
            command.Key = key;
            var requestValidationResult = _requestValidator.ValidateRequest(command);
            if (!requestValidationResult.IsSuccessful)
            {
                return requestValidationResult.ToResultIfValid<UpdatePlexServerLibraryCommand, BadRequestResult>();
            }

            var resultContext = await _mediator.Send(command);

            return resultContext.ToResultIfValid<NoContentResult>();
        }

        [FunctionName("SyncLibraries")]
        public async Task<IActionResult> SyncLibraries(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "plex/libraries/sync")]
            HttpRequest req)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req, PlexRequestRoles.Admin);

            if (accessResult.Status == AccessTokenStatus.InsufficientPermissions)
            {
                return new ForbidResult();
            }

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }


            var resultContext = await _mediator.Send(new SyncLibrariesCommand());

            return resultContext.ToResultIfValid<NoContentResult>();
        }

        [FunctionName("SyncContent")]
        public async Task<IActionResult> SyncContent(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "plex/content/sync")]
            HttpRequest req)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req, PlexRequestRoles.Admin);

            if (accessResult.Status == AccessTokenStatus.InsufficientPermissions)
            {
                return new ForbidResult();
            }

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            var fullRefresh = false;
            if (bool.TryParse(req.Query["fullRefresh"], out var requestedFullRefresh))
            {
                fullRefresh = requestedFullRefresh;
            }

            var command = new SyncContentCommand(fullRefresh);
            var requestValidationResult = _requestValidator.ValidateRequest(command);
            if (!requestValidationResult.IsSuccessful)
            {
                return requestValidationResult.ToResultIfValid<SyncContentCommand, BadRequestResult>();
            }

            await _mediator.Send(command);

            return new NoContentResult();
        }

        [FunctionName("GetServer")]
        public async Task<IActionResult> GetServer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "plex/server")]
            HttpRequest req)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req, PlexRequestRoles.Admin);

            if (accessResult.Status == AccessTokenStatus.InsufficientPermissions)
            {
                return new ForbidResult();
            }

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            var result = await _mediator.Send(new GetServerQuery());

            return new OkObjectResult(result);
        }
    }
}
