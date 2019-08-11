using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlexRequests.ApiRequests.Requests.Commands;
using PlexRequests.ApiRequests.Requests.Queries;
using PlexRequests.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace PlexRequests.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class IssueController : Controller
    {
        private readonly IMediator _mediator;

        public IssueController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("")]
        [SwaggerResponse(204)]
        [SwaggerResponse(400)]
        [SwaggerResponse(401)]
        [SwaggerResponse(424)]
        public async Task<ActionResult> CreateIssue([FromBody] CreateIssueCommand command)
        {
            await _mediator.Send(command);

            return NoContent();
        }

        [HttpPut("{id:guid}")]
        [SwaggerResponse(204)]
        [SwaggerResponse(400)]
        [SwaggerResponse(401)]
        [Admin]
        public async Task<ActionResult> UpdateIssue([FromRoute][Required] Guid id, [FromBody] UpdateIssueCommand command)
        {
            command.Id = id;
            await _mediator.Send(command);

            return NoContent();
        }

        [HttpPost("{id:guid}/Comment")]
        [SwaggerResponse(204)]
        [SwaggerResponse(400)]
        [SwaggerResponse(401)]
        [SwaggerResponse(424)]
        public async Task<ActionResult> CreateComment([FromRoute][Required] Guid id, [FromBody] CreateIssueCommentCommand command)
        {
            command.Id = id;
            await _mediator.Send(command);

            return NoContent();
        }

        [HttpGet("{id:guid}")]
        [SwaggerResponse(204)]
        [SwaggerResponse(400)]
        [SwaggerResponse(401)]
        [SwaggerResponse(424)]
        public async Task<ActionResult> GetIssue([FromRoute][Required] Guid id)
        {
            var query = new GetOneIssueQuery(id);
            var result = await _mediator.Send(query);

            return Ok(result.Issue);
        }

        [HttpGet("")]
        [SwaggerResponse(200, null, typeof(GetPagedIssueQueryResult))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<GetPagedIssueQueryResult>> GetPagedIssues([FromQuery] int? page, [FromQuery] int? pageSize, [FromQuery] bool includeResolved = false)
        {
            var query = new GetPagedIssueQuery
            {
                Page = page,
                PageSize = pageSize,
                IncludeResolved = includeResolved
            };

            var response = await _mediator.Send(query);

            return Ok(response);
        }
    }
}