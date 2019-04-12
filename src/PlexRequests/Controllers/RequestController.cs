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

        [HttpGet("")]
        public async Task<ActionResult<GetPagedRequestQueryResult>> GetRequests([FromQuery] string title,
            [FromQuery] PlexMediaTypes? mediaType, [FromQuery] bool? includeCurrentUsersOnly, [FromQuery] bool? isApproved,
            [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var query = new GetPagedRequestQuery
            {
                Title = title,
                MediaType = mediaType,
                IncludeCurrentUsersOnly = includeCurrentUsersOnly,
                IsApproved = isApproved,
                Page = page,
                PageSize = pageSize
            };

            var response = await _mediator.Send(query);

            return Ok(response);
        }
    }
}