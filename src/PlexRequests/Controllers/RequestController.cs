using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlexRequests.ApiRequests.Requests.Commands;
using PlexRequests.ApiRequests.Requests.Queries;
using PlexRequests.Attributes;
using PlexRequests.Repository.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace PlexRequests.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class RequestController : Controller
    {
        private readonly IMediator _mediator;

        public RequestController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Movie")]
        [SwaggerResponse(204)]
        [SwaggerResponse(400, null, typeof(ApiErrorResponse))]
        [SwaggerResponse(401)]
        public async Task<ActionResult> CreateMovieRequest([FromBody] CreateMovieRequestCommand command)
        {
            await _mediator.Send(command);

            return NoContent();
        }

        [HttpPost("Movie/{id:guid}/Approve")]
        [Admin]
        [SwaggerResponse(204)]
        [SwaggerResponse(400, null, typeof(ApiErrorResponse))]
        [SwaggerResponse(401)]
        [SwaggerResponse(404)]
        public async Task<ActionResult> ApproveMovieRequest([FromRoute] Guid id)
        {
            var command = new ApproveMovieRequestCommand(id);

            await _mediator.Send(command);

            return NoContent();
        }

        [HttpPost("Movie/{id:guid}/Reject")]
        [Admin]
        [SwaggerResponse(204)]
        [SwaggerResponse(400, null, typeof(ApiErrorResponse))]
        [SwaggerResponse(401)]
        [SwaggerResponse(404)]
        public async Task<ActionResult> RejectMovieRequest([FromRoute] Guid id, [FromBody] RejectMovieRequestCommand command)
        {
            command.RequestId = id;

            await _mediator.Send(command);

            return NoContent();
        }

        [HttpPost("Tv")]
        [SwaggerResponse(204)]
        [SwaggerResponse(400, null, typeof(ApiErrorResponse))]
        [SwaggerResponse(401)]
        public async Task<ActionResult> CreateTvRequest([FromBody] CreateTvRequestCommand command)
        {
            await _mediator.Send(command);

            return NoContent();
        }

        [HttpPost("Tv/{id:guid}/Approve")]
        [Admin]
        [SwaggerResponse(204)]
        [SwaggerResponse(400, null, typeof(ApiErrorResponse))]
        [SwaggerResponse(401)]
        [SwaggerResponse(404)]
        public async Task<ActionResult> ApproveTvRequest([FromRoute] Guid id, [FromBody] ApproveTvRequestCommand command)
        {
            command.RequestId = id;

            await _mediator.Send(command);

            return NoContent();
        }

        [HttpPost("Tv/{id:guid}/Reject")]
        [Admin]
        [SwaggerResponse(204)]
        [SwaggerResponse(400, null, typeof(ApiErrorResponse))]
        [SwaggerResponse(401)]
        [SwaggerResponse(404)]
        public async Task<ActionResult> RejectTvRequest([FromRoute] Guid id, [FromBody] RejectTvRequestCommand command)
        {
            command.RequestId = id;

            await _mediator.Send(command);

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [SwaggerResponse(204)]
        [SwaggerResponse(400, null, typeof(ApiErrorResponse))]
        [SwaggerResponse(401)]
        [SwaggerResponse(403)]
        [SwaggerResponse(404)]
        public async Task<ActionResult> DeleteRequest([FromRoute][Required] Guid id)
        {
            var command = new DeleteRequestCommand(id);

            await _mediator.Send(command);

            return NoContent();
        }

        [HttpGet("Movie")]
        [SwaggerResponse(200, null, typeof(GetMoviePagedRequestQueryResult))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<GetMoviePagedRequestQueryResult>> GetMovieRequests([FromQuery] GetMoviePagedRequestQuery query)
        {
            var response = await _mediator.Send(query);

            return Ok(response);
        }

        [HttpGet("Tv")]
        [SwaggerResponse(200, null, typeof(GetTvPagedRequestQueryResult))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<GetTvPagedRequestQueryResult>> GetTvRequests([FromQuery] GetTvPagedRequestQuery query)
        {
            var response = await _mediator.Send(query);

            return Ok(response);
        }
    }
}