using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlexRequests.ApiRequests.Auth.Commands;
using Swashbuckle.AspNetCore.Annotations;

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
        [SwaggerResponse(200, "Login was successful")]
        [SwaggerResponse(400, "Login unsuccessful due to invalid account details")]
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
        [SwaggerOperation(
            Description = "Creates and Admin account. Only one Admin account can exist at any time.")]
        [SwaggerResponse(200, "Admin created successfully")]
        [SwaggerResponse(400, "Unable to create Admin account")]
        public async Task<ActionResult<UserLoginCommandResult>> AddPlexAdmin([FromBody] AddAdminCommand command)
        {
            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}
