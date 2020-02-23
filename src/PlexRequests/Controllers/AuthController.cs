using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlexRequests.ApiRequests.Auth.Commands;
using Swashbuckle.AspNetCore.Annotations;

namespace PlexRequests.Controllers
{
    [Route("api/[controller]")]
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
        [SwaggerResponse(400, "Login unsuccessful due to invalid account details", typeof(ApiErrorResponse))]
        public async Task<ActionResult<UserLoginCommandResult>> Login([FromBody] UserLoginCommand command)
        {
            var result = await _mediator.Send(command);

            if (result == null)
            {
                return Unauthorized();
            }

            return Ok(result);
        }

        [HttpPost("Refresh")]
        [AllowAnonymous]
        [SwaggerResponse(200, "Refresh was successful")]
        [SwaggerResponse(400, "Refresh unsuccessful due to invalid credentials or bad refresh token", typeof(ApiErrorResponse))]
        public async Task<ActionResult<UserLoginCommandResult>> Refresh([FromBody] RefreshTokenCommand command)
        {
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpPost("CreateAdmin")]
        [AllowAnonymous]
        [SwaggerOperation(
            Description = "Creates and Admin account. Only one Admin account can exist at any time.")]
        [SwaggerResponse(200, "Admin created successfully", typeof(UserLoginCommandResult))]
        [SwaggerResponse(400, "Unable to create Admin account", typeof(ApiErrorResponse))]
        public async Task<ActionResult<UserLoginCommandResult>> AddPlexAdmin([FromBody] AddAdminCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.IsSuccessful)
            {
                return BadRequest(result.ValidationErrors[0].Message);
            }

            return Ok(result);
        }
    }
}
