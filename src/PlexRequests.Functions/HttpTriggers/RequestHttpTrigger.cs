using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using PlexRequests.Core.Auth;
using PlexRequests.DataAccess.Enums;
using PlexRequests.Functions.AccessTokens;
using PlexRequests.Functions.Features.Requests.Commands;
using PlexRequests.Functions.Features.Requests.Queries;

namespace PlexRequests.Functions.HttpTriggers
{
    public class RequestHttpTrigger
    {
        private readonly IMediator _mediator;
        private readonly IRequestValidator _requestValidator;
        private readonly IAccessTokenProvider _accessTokenProvider;

        public RequestHttpTrigger(
            IMediator mediator,
            IRequestValidator requestValidator,
            IAccessTokenProvider accessTokenProvider
        )
        {
            _mediator = mediator;
            _requestValidator = requestValidator;
            _accessTokenProvider = accessTokenProvider;
        }

        [FunctionName("CreateMovieRequest")]
        public async Task<IActionResult> CreateMovieRequest(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "request/movie")]
            HttpRequest req)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req, PlexRequestRoles.Admin);

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            var command = await req.DeserializeAndValidateRequest<CreateMovieRequestCommand>();
            var requestValidationResult = _requestValidator.ValidateRequest(command);
            if (!requestValidationResult.IsSuccessful)
            {
                return requestValidationResult.ToResultIfValid<CreateMovieRequestCommand, BadRequestResult>();
            }

            var resultContext = await _mediator.Send(command);

            return resultContext.ToResultIfValid<NoContentResult>();
        }

        [FunctionName("ApproveMovieRequest")]
        public async Task<IActionResult> ApproveMovieRequest(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "request/movie/{id:int}/approve")]
            HttpRequest req, int id)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req, PlexRequestRoles.Admin);

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            var command = await req.DeserializeAndValidateRequest<CreateMovieRequestCommand>();
            command.TheMovieDbId = id;
            var requestValidationResult = _requestValidator.ValidateRequest(command);
            if (!requestValidationResult.IsSuccessful)
            {
                return requestValidationResult.ToResultIfValid<CreateMovieRequestCommand, BadRequestResult>();
            }

            var resultContext = await _mediator.Send(command);

            return resultContext.ToResultIfValid<NoContentResult>();
        }

        [FunctionName("RejectMovieRequest")]
        public async Task<IActionResult> RejectMovieRequest(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "request/movie/{id:int}/reject")]
            HttpRequest req, int id)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req, PlexRequestRoles.Admin);

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            var command = await req.DeserializeAndValidateRequest<RejectMovieRequestCommand>();
            command.RequestId = id;
            var requestValidationResult = _requestValidator.ValidateRequest(command);
            if (!requestValidationResult.IsSuccessful)
            {
                return requestValidationResult.ToResultIfValid<RejectMovieRequestCommand, BadRequestResult>();
            }

            var resultContext = await _mediator.Send(command);

            return resultContext.ToResultIfValid<NoContentResult>();
        }

        [FunctionName("CreateTvRequest")]
        public async Task<IActionResult> CreateTvRequest(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "request/tv")]
            HttpRequest req)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req);

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            var command = await req.DeserializeAndValidateRequest<CreateTvRequestCommand>();
            var requestValidationResult = _requestValidator.ValidateRequest(command);
            if (!requestValidationResult.IsSuccessful)
            {
                return requestValidationResult.ToResultIfValid<CreateTvRequestCommand, BadRequestResult>();
            }

            var resultContext = await _mediator.Send(command);

            return resultContext.ToResultIfValid<NoContentResult>();
        }

        [FunctionName("ApproveTvRequest")]
        public async Task<IActionResult> ApproveTvRequest(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "request/tv/{id:int}/approve")]
            HttpRequest req, int id)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req, PlexRequestRoles.Admin);

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            var command = await req.DeserializeAndValidateRequest<ApproveTvRequestCommand>();
            command.RequestId = id;
            var requestValidationResult = _requestValidator.ValidateRequest(command);
            if (!requestValidationResult.IsSuccessful)
            {
                return requestValidationResult.ToResultIfValid<ApproveTvRequestCommand, BadRequestResult>();
            }

            var resultContext = await _mediator.Send(command);

            return resultContext.ToResultIfValid<NoContentResult>();
        }

        [FunctionName("RejectTvRequest")]
        public async Task<IActionResult> RejectTvRequest(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "request/tv/{id:int}/reject")]
            HttpRequest req, int id)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req, PlexRequestRoles.Admin);

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            var command = await req.DeserializeAndValidateRequest<RejectTvRequestCommand>();
            command.RequestId = id;
            var requestValidationResult = _requestValidator.ValidateRequest(command);
            if (!requestValidationResult.IsSuccessful)
            {
                return requestValidationResult.ToResultIfValid<RejectTvRequestCommand, BadRequestResult>();
            }

            var resultContext = await _mediator.Send(command);

            return resultContext.ToResultIfValid<NoContentResult>();
        }

        [FunctionName("DeleteMovieRequest")]
        public async Task<IActionResult> DeleteMovieRequest(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "request/movie/{id:int}")]
            HttpRequest req, int id)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req);

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            var command = new DeleteMovieRequestCommand(id);
            var requestValidationResult = _requestValidator.ValidateRequest(command);
            if (!requestValidationResult.IsSuccessful)
            {
                return requestValidationResult.ToResultIfValid<DeleteMovieRequestCommand, BadRequestResult>();
            }

            var resultContext = await _mediator.Send(command);

            return resultContext.ToResultIfValid<NoContentResult>();
        }

        [FunctionName("GetMovieRequests")]
        public async Task<IActionResult> GetMovieRequests(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "request/movie")]
            HttpRequest req)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req);

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            string title = req.Query["title"];
            RequestStatuses? status = null;
            bool? includeCurrentUsersOnly = null;
            int? page = null;
            int? pageSize = null;

            if (Enum.TryParse<RequestStatuses>(req.Query["status"], out var requestedStatus))
            {
                status = requestedStatus;
            }

            if (bool.TryParse(req.Query["includeCurrentUsersOnly"], out var requestedIncludeCurrentUsersOnly))
            {
                includeCurrentUsersOnly = requestedIncludeCurrentUsersOnly;
            }

            if (int.TryParse(req.Query["page"], out var requestedPage))
            {
                page = requestedPage;
            }

            if (int.TryParse(req.Query["pageSize"], out var requestedPageSize))
            {
                pageSize = requestedPageSize;
            }

            var query = new GetMoviePagedRequestQuery
            {
                Title = title,
                Status = status,
                IncludeCurrentUsersOnly = includeCurrentUsersOnly,
                Page = page,
                PageSize = pageSize
            };

            var requestValidationResult = _requestValidator.ValidateRequest(query);
            if (!requestValidationResult.IsSuccessful)
            {
                return requestValidationResult.ToResultIfValid<GetMoviePagedRequestQuery, BadRequestResult>();
            }

            var resultContext = await _mediator.Send(query);

            return new OkObjectResult(resultContext);
        }

        [FunctionName("GetTvRequests")]
        public async Task<IActionResult> GetTvRequests(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "request/tv")]
            HttpRequest req)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req);

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            string title = req.Query["title"];
            RequestStatuses? status = null;
            bool? includeCurrentUsersOnly = null;
            int? page = null;
            int? pageSize = null;

            if (Enum.TryParse<RequestStatuses>(req.Query["status"], out var requestedStatus))
            {
                status = requestedStatus;
            }

            if (bool.TryParse(req.Query["includeCurrentUsersOnly"], out var requestedIncludeCurrentUsersOnly))
            {
                includeCurrentUsersOnly = requestedIncludeCurrentUsersOnly;
            }

            if (int.TryParse(req.Query["page"], out var requestedPage))
            {
                page = requestedPage;
            }

            if (int.TryParse(req.Query["pageSize"], out var requestedPageSize))
            {
                pageSize = requestedPageSize;
            }

            var query = new GetTvPagedRequestQuery
            {
                Title = title,
                Status = status,
                IncludeCurrentUsersOnly = includeCurrentUsersOnly,
                Page = page,
                PageSize = pageSize
            };

            var requestValidationResult = _requestValidator.ValidateRequest(query);
            if (!requestValidationResult.IsSuccessful)
            {
                return requestValidationResult.ToResultIfValid<GetTvPagedRequestQuery, BadRequestResult>();
            }

            var resultContext = await _mediator.Send(query);

            return new OkObjectResult(resultContext);
        }

        [FunctionName("DeleteTvRequest")]
        public async Task<IActionResult> DeleteTvRequest(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "request/tv/{id:int}")]
            HttpRequest req, int id)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req);

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            var command = new DeleteTvRequestCommand(id);
            var requestValidationResult = _requestValidator.ValidateRequest(command);
            if (!requestValidationResult.IsSuccessful)
            {
                return requestValidationResult.ToResultIfValid<DeleteTvRequestCommand, BadRequestResult>();
            }

            var resultContext = await _mediator.Send(command);

            return resultContext.ToResultIfValid<NoContentResult>();
        }

    }
}
