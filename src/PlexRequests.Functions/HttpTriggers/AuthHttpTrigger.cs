using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using PlexRequests.Functions.Features.Auth.Commands;

namespace PlexRequests.Functions.HttpTriggers
{
    public class AuthHttpTrigger
    {
        private readonly IMediator _mediator;
        private readonly IRequestValidator _requestValidator;

        public AuthHttpTrigger(
            IMediator mediator,
            IRequestValidator requestValidator
        )
        {
            _mediator = mediator;
            _requestValidator = requestValidator;
        }

        [FunctionName("Login")]
        public async Task<IActionResult> Login(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")]
            HttpRequest req)
        {
            var command = await req.DeserializeAndValidateRequest<UserLoginCommand>();
            var requestValidationResult = _requestValidator.ValidateRequest(command);
            if (!requestValidationResult.IsSuccessful)
            {
                return requestValidationResult.ToResultIfValid<UserLoginCommand, BadRequestResult>();
            }

            var resultContext = await _mediator.Send(command);

            return resultContext.ToOkIfValidResult();
        }

        [FunctionName("Refresh")]
        public async Task<IActionResult> Refresh(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/refresh")]
            HttpRequest req)
        {
            var command = await req.DeserializeAndValidateRequest<RefreshTokenCommand>();
            var requestValidationResult = _requestValidator.ValidateRequest(command);
            if (!requestValidationResult.IsSuccessful)
            {
                return requestValidationResult.ToResultIfValid<RefreshTokenCommand, BadRequestResult>();
            }

            var resultContext = await _mediator.Send(command);

            return resultContext.ToOkIfValidResult();
        }

        [FunctionName("CreateAdmin")]
        public async Task<IActionResult> CreateAdmin(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/createadmin")]
            HttpRequest req)
        {
            var command = await req.DeserializeAndValidateRequest<AddAdminCommand>();
            var requestValidationResult = _requestValidator.ValidateRequest(command);
            if (!requestValidationResult.IsSuccessful)
            {
                return requestValidationResult.ToResultIfValid<AddAdminCommand, BadRequestResult>();
            }

            var resultContext = await _mediator.Send(command);

            return resultContext.ToOkIfValidResult();
        }
    }
}
