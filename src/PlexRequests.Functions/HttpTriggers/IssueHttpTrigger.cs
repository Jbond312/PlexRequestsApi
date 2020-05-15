using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using PlexRequests.Core.Auth;
using PlexRequests.Functions.AccessTokens;
using PlexRequests.Functions.Features.Issues.Commands;
using PlexRequests.Functions.Features.Issues.Queries;

namespace PlexRequests.Functions.HttpTriggers
{
    public class IssueHttpTrigger
    {
        private readonly IMediator _mediator;
        private readonly IRequestValidator _requestValidator;
        private readonly IAccessTokenProvider _accessTokenProvider;

        public IssueHttpTrigger(
            IMediator mediator,
            IRequestValidator requestValidator,
            IAccessTokenProvider accessTokenProvider
            )
        {
            _mediator = mediator;
            _requestValidator = requestValidator;
            _accessTokenProvider = accessTokenProvider;
        }

        [FunctionName("CreateIssue")]
        public async Task<IActionResult> CreateIssue(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "issue")]
            HttpRequest req)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req);

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            var command = await req.DeserializeAndValidateRequest<CreateIssueCommand>();
            command.UserInfo = accessResult.UserInfo;

            var requestValidationResult = _requestValidator.ValidateRequest(command);
            if (!requestValidationResult.IsSuccessful)
            {
                return requestValidationResult.ToResultIfValid<CreateIssueCommand, BadRequestResult>();
            }

            var resultContext = await _mediator.Send(command);

            return resultContext.ToOkIfValidResult();
        }

        [FunctionName("UpdateIssue")]
        public async Task<IActionResult> UpdateIssue(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "issue/{id:int}")]
            HttpRequest req, int id)
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

            var command = await req.DeserializeAndValidateRequest<UpdateIssueCommand>();
            command.Id = id;
            var requestValidationResult = _requestValidator.ValidateRequest(command);
            if (!requestValidationResult.IsSuccessful)
            {
                return requestValidationResult.ToResultIfValid<UpdateIssueCommand, BadRequestResult>();
            }

            var resultContext = await _mediator.Send(command);

            return resultContext.ToOkIfValidResult();
        }

        [FunctionName("CreateIssueComment")]
        public async Task<IActionResult> CreateIssueComment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "issue/{id:int}/comment")]
            HttpRequest req, int id)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req, PlexRequestRoles.Commenter);

            if (accessResult.Status == AccessTokenStatus.InsufficientPermissions)
            {
                return new ForbidResult();
            }

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            var command = await req.DeserializeAndValidateRequest<CreateIssueCommentCommand>();
            command.Id = id;
            command.UserInfo = accessResult.UserInfo;

            var requestValidationResult = _requestValidator.ValidateRequest(command);
            if (!requestValidationResult.IsSuccessful)
            {
                return requestValidationResult.ToResultIfValid<CreateIssueCommentCommand, BadRequestResult>();
            }

            var resultContext = await _mediator.Send(command);

            return resultContext.ToResultIfValid<NoContentResult>();
        }

        [FunctionName("GetIssue")]
        public async Task<IActionResult> GetIssue(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "issue/{id:int}")]
            HttpRequest req, int id)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req);

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            var query = new GetOneIssueQuery(id);
            var requestValidationResult = _requestValidator.ValidateRequest(query);
            if (!requestValidationResult.IsSuccessful)
            {
                return requestValidationResult.ToResultIfValid<GetOneIssueQuery, BadRequestResult>();
            }

            var resultContext = await _mediator.Send(query);

            return resultContext.ToOkIfValidResult();
        }

        [FunctionName("GetAllIssues")]
        public async Task<IActionResult> GetAllIssues(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "issue")]
            HttpRequest req)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req);

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            int? page = null;
            int? pageSize = null;
            var includeResolved = false;

            if (int.TryParse(req.Query["page"], out var requestedPage))
            {
                page = requestedPage;
            }

            if (int.TryParse(req.Query["pageSize"], out var requestedPageSize))
            {
                pageSize = requestedPageSize;
            }

            if (bool.TryParse(req.Query["includeResolved"], out var requestedIncludeResolved))
            {
                includeResolved = requestedIncludeResolved;
            }

            var query = new GetPagedIssueQuery
            {
                IncludeResolved = includeResolved,
                Page = page,
                PageSize = pageSize
            };

            var requestValidationResult = _requestValidator.ValidateRequest(query);
            if (!requestValidationResult.IsSuccessful)
            {
                return requestValidationResult.ToResultIfValid<GetPagedIssueQuery, BadRequestResult>();
            }

            var resultContext = await _mediator.Send(query);

            return resultContext.ToOkIfValidResult();
        }
    }
}
