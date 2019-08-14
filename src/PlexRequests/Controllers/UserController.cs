using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlexRequests.ApiRequests.Users.Commands;
using PlexRequests.ApiRequests.Users.Queries;
using PlexRequests.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace PlexRequests.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : Controller
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("")]
        [Admin]
        [SwaggerResponse(200, null, typeof(GetManyUserQueryResult))]
        [SwaggerResponse(400, null, typeof(ApiErrorResponse))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<GetManyUserQueryResult>> GetUsers([FromQuery] GetManyUserQuery query)
        {
            var users = await _mediator.Send(query);

            return Ok(users);
        }

        [HttpPut]
        [Route("{id:guid}")]
        [Admin]
        [SwaggerResponse(204)]
        [SwaggerResponse(400, null, typeof(ApiErrorResponse))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<GetManyUserQueryResult>> UpdateUser([FromRoute] Guid id, [FromBody] UpdateUserCommand command)
        {
            command.Id = id;
            await _mediator.Send(command);

            return NoContent();
        }
    }
}
