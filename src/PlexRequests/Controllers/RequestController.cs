using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlexRequests.Models.Requests;

namespace PlexRequests.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RequestController : Controller
    {
        private readonly IMediator _mediator;

        public RequestController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Movie")]
        public async Task<ActionResult> Create([FromBody] CreateMovieRequestCommand command)
        {
            await _mediator.Send(command);

            return Ok();
        }
    }
}