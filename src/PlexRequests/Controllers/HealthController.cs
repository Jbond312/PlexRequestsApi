using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PlexRequests.ApiRequests.Health.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace PlexRequests.Controllers
{
    [Route("api/[controller]")]
    public class HealthController : Controller
    {
        private readonly IMediator _mediator;

        public HealthController(
            IMediator mediator
        )
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(200)]
        public async Task<ActionResult<GetHealthQueryResult>> HealthCheck()
        {
            return await _mediator.Send(new GetHealthQuery());
        }
    }
}