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
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(401)]
        public async Task<ActionResult> CreateMovieRequest([FromBody] CreateMovieRequestCommand command)
        {
            await _mediator.Send(command);

            return Ok();
        }
        
        [HttpPost("Movie/{id:guid}/Approve")]
        [Admin]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(401)]
        public async Task<ActionResult> ApproveMovieRequest([FromRoute] Guid id)
        {
            var command = new ApproveMovieRequestCommand(id);
            
            await _mediator.Send(command);

            return Ok();
        }
        
        [HttpPost("Tv")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(401)]
        public async Task<ActionResult> CreateTvRequest([FromBody] CreateTvRequestCommand command)
        {
            await _mediator.Send(command);

            return Ok();
        }
        
        [HttpPost("Tv/{id:guid}/Approve")]
        [Admin]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(401)]
        public async Task<ActionResult> ApproveTvRequest([FromRoute] Guid id, ApproveTvRequestCommand command)
        {
            command.RequestId = id;
            
            await _mediator.Send(command);

            return Ok();
        }

        [HttpDelete("{id:guid}")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(401)]
        [SwaggerResponse(403)]
        public async Task<ActionResult> DeleteRequest([FromRoute][Required] Guid id)
        {
            var command = new DeleteRequestCommand(id);

            await _mediator.Send(command);
            
            return Ok();
        }

        [HttpGet("Movie")]
        [SwaggerResponse(200, null, typeof(GetMoviePagedRequestQueryResult))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<GetMoviePagedRequestQueryResult>> GetMovieRequests([FromQuery] string title,
            [FromQuery] bool? includeCurrentUsersOnly, [FromQuery] RequestStatuses? status,
            [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var query = new GetMoviePagedRequestQuery
            {
                Title = title,
                IncludeCurrentUsersOnly = includeCurrentUsersOnly,
                Status = status,
                Page = page,
                PageSize = pageSize
            };

            var response = await _mediator.Send(query);

            return Ok(response);
        }

        [HttpGet("Tv")]
        [SwaggerResponse(200, null, typeof(GetTvPagedRequestQueryResult))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<GetTvPagedRequestQueryResult>> GetTvRequests([FromQuery] string title,
            [FromQuery] bool? includeCurrentUsersOnly, [FromQuery] RequestStatuses? status,
            [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var query = new GetTvPagedRequestQuery
            {
                Title = title,
                IncludeCurrentUsersOnly = includeCurrentUsersOnly,
                Status = status,
                Page = page,
                PageSize = pageSize
            };

            var response = await _mediator.Send(query);

            return Ok(response);
        }
    }
}