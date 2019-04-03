using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlexRequests.Models.Auth;

namespace PlexRequests.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthController : Controller
    {
        private readonly IMediator _mediator;

        public AuthController(
            IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<ActionResult<UserLoginCommandResult>> Login([FromBody] UserLoginCommand command)
        {
            var result = await _mediator.Send(command);

            if (result == null)
            {
                return Unauthorized();
            }

            return Ok(result);
        }

        [HttpPost("CreateAdmin")]
        [AllowAnonymous]
        public async Task<ActionResult<UserLoginCommandResult>> AddPlexAdmin([FromBody] AddAdminCommand command)
        {
            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}
