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

        public AuthHttpTrigger(
            IMediator mediator
        )
        {
            _mediator = mediator;
        }

        [FunctionName("Login")]
        public async Task<IActionResult> Login(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")]
            HttpRequest req)
        {
            var command = await req.DeserializeBody<UserLoginCommand>();

            var result = await _mediator.Send(command);

            return result.ToOkIfValidResult();
        }
    }
}
