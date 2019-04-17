using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlexRequests.Models.Requests;
using PlexRequests.Store.Enums;

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
        public async Task<ActionResult> CreateMovieRequest([FromBody] CreateMovieRequestCommand command)
        {
            await _mediator.Send(command);

            return Ok();
        }
        
        [HttpPost("Tv")]
        public async Task<ActionResult> CreateTvRequest([FromBody] CreateTvRequestCommand command)
        {
            await _mediator.Send(command);

            return Ok();
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteRequest([FromRoute][Required] Guid id)
        {
            var command = new DeleteRequestCommand(id);

            await _mediator.Send(command);
            
            return Ok();
        }

        [HttpGet("Movie")]
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