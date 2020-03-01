using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using PlexRequests.Functions.Features.Health.Queries;

namespace PlexRequests.Functions.HttpTriggers
{
    public class HealthHttpTrigger
    {
        private readonly IMediator _mediator;

        public HealthHttpTrigger(
            IMediator mediator
            )
        {
            _mediator = mediator;
        }

        [FunctionName("GetHealth")]
        public async Task<IActionResult> HealthCheck(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")]
            HttpRequest req)
        {
            var result = await _mediator.Send(new GetHealthQuery());

            return new OkObjectResult(result);
        }
    }
}
