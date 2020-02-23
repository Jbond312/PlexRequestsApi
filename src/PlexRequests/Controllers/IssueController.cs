using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlexRequests.ApiRequests.Issues.Commands;
using PlexRequests.ApiRequests.Issues.Models.Detail;
using PlexRequests.ApiRequests.Issues.Queries;
using PlexRequests.Attributes;
using PlexRequests.Core.Auth;
using Swashbuckle.AspNetCore.Annotations;

namespace PlexRequests.Controllers
{
    [Route("api/[controller]")]
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
        [SwaggerResponse(400, null, typeof(ApiErrorResponse))]
        [SwaggerResponse(401)]
        [SwaggerResponse(424)]
        public async Task<ActionResult> CreateIssue([FromBody] CreateIssueCommand command)
        {
            await _mediator.Send(command);

            return NoContent();
        }

        [HttpPut("{id:int}")]
        [SwaggerResponse(204)]
        [SwaggerResponse(400, null, typeof(ApiErrorResponse))]
        [SwaggerResponse(401)]
        [Admin]
        public async Task<ActionResult> UpdateIssue([FromRoute][Required] int id, [FromBody] UpdateIssueCommand command)
        {
            command.Id = id;
            await _mediator.Send(command);

            return NoContent();
        }

        [HttpPost("{id:int}/Comment")]
        [Authorize(Roles = PlexRequestRoles.Commenter)]
        [SwaggerResponse(204)]
        [SwaggerResponse(400, null, typeof(ApiErrorResponse))]
        [SwaggerResponse(401)]
        [SwaggerResponse(424)]
        public async Task<ActionResult> CreateComment([FromRoute][Required] int id, [FromBody] CreateIssueCommentCommand command)
        {
            command.Id = id;
            await _mediator.Send(command);

            return NoContent();
        }

        [HttpGet("{id:int}")]
        [SwaggerResponse(200, null, typeof(IssueDetailModel))]
        [SwaggerResponse(400, null, typeof(ApiErrorResponse))]
        [SwaggerResponse(401)]
        [SwaggerResponse(424)]
        public async Task<ActionResult<IssueDetailModel>> GetIssue([FromRoute][Required] int id)
        {
            var query = new GetOneIssueQuery(id);
            var result = await _mediator.Send(query);

            return Ok(result.Issue);
        }

        [HttpGet("")]
        [SwaggerResponse(200, null, typeof(GetPagedIssueQueryResult))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<GetPagedIssueQueryResult>> GetPagedIssues([FromQuery] GetPagedIssueQuery query)
        {
            var response = await _mediator.Send(query);

            return Ok(response);
        }
    }
}