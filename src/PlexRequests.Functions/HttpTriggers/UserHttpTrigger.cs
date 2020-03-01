using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using PlexRequests.Functions.AccessTokens;
using PlexRequests.Functions.Features.Users.Commands;
using PlexRequests.Functions.Features.Users.Queries;

namespace PlexRequests.Functions.HttpTriggers
{
    public class UserHttpTrigger
    {
        private readonly IMediator _mediator;
        private readonly IRequestValidator _requestValidator;
        private readonly IAccessTokenProvider _accessTokenProvider;

        public UserHttpTrigger(
            IMediator mediator,
            IRequestValidator requestValidator,
            IAccessTokenProvider accessTokenProvider
        )
        {
            _mediator = mediator;
            _requestValidator = requestValidator;
            _accessTokenProvider = accessTokenProvider;
        }

        [FunctionName("GetUsers")]
        public async Task<IActionResult> GetUsers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user")]
            HttpRequest req)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req);

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            var includeDisabled = false;
            if (bool.TryParse(req.Query["includeDisabled"], out var requestedIncludeDisabled))
            {
                includeDisabled = requestedIncludeDisabled;
            }

            var query = new GetManyUserQuery
            {
                IncludeDisabled = includeDisabled
            };

            var requestValidationResult = _requestValidator.ValidateRequest(query);
            if (!requestValidationResult.IsSuccessful)
            {
                return requestValidationResult.ToResultIfValid<GetManyUserQuery, BadRequestResult>();
            }

            var resultContext = await _mediator.Send(query);

            return new OkObjectResult(resultContext.Users);
        }

        [FunctionName("UpdateUser")]
        public async Task<IActionResult> UpdateUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "user/{id:int}")]
            HttpRequest req, int id)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req);

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            var command = await req.DeserializeAndValidateRequest<UpdateUserCommand>();
            command.Id = id;
            var requestValidationResult = _requestValidator.ValidateRequest(command);
            if (!requestValidationResult.IsSuccessful)
            {
                return requestValidationResult.ToResultIfValid<UpdateUserCommand, BadRequestResult>();
            }

            var resultContext = await _mediator.Send(command);

            return resultContext.ToResultIfValid<NoContentResult>();
        }
    }
}