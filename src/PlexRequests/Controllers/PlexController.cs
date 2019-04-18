using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlexRequests.ApiRequests.Plex.Commands;
using PlexRequests.ApiRequests.Plex.DTOs.Detail;
using PlexRequests.ApiRequests.Plex.Queries;
using PlexRequests.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace PlexRequests.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlexController : Controller
    {
        private readonly IMediator _mediator;

        public PlexController(
            IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Users/Sync")]
        [Admin]
        [SwaggerOperation(
            Description = "Synchronise all users associated with the Admin account. Sync can be called many times to refresh users.")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(401)]
        public async Task<ActionResult> SyncUsers()
        {
            await _mediator.Send(new SyncUsersCommand());

            return Ok();
        }

        [HttpGet]
        [Route("Libraries")]
        [Admin]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(401)]
        public async Task<ActionResult<GetManyPlexServerLibraryQueryResult>> GetLibraries()
        {
            var query = new GetManyPlexServerLibraryQuery();

            var libraries = await _mediator.Send(query);
            
            return Ok(libraries);
        }
        
        [HttpPut]
        [Route("Libraries/{key}")]
        [Admin]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(401)]
        public async Task<ActionResult> UpdateLibrary(string key, UpdatePlexServerLibraryCommand command)
        {
            command.Key = key;

            var libraries = await _mediator.Send(command);
            
            return Ok(libraries);
        }
        
        [HttpPost("Libraries/Sync")]
        [Admin]
        [SwaggerOperation(
            Description = "Synchronise all libraries associated with the Admin account. Sync can be called many times to refresh libraries.")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(401)]
        public async Task<ActionResult> SyncLibraries()
        {
            await _mediator.Send(new SyncLibrariesCommand());

            return Ok();
        }

        [HttpPost("Content/Sync")]
        [Admin]
        [SwaggerOperation(
            Description = "Synchronise all Plex media content from any 'Enabled' library in PlexRequests.")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(401)]
        public async Task<ActionResult> SyncContent([FromQuery] bool fullRefresh = false)
        {
            var command = new SyncContentCommand(fullRefresh);

            await _mediator.Send(command);

            return Ok();
        }

        [HttpGet("Server")]
        [Admin]
        [SwaggerResponse(200, null, typeof(PlexServerDetailModel))]
        [SwaggerResponse(400)]
        [SwaggerResponse(401)]
        public async Task<ActionResult<PlexServerDetailModel>> GetServer()
        {
            var query = new GetServerQuery();

            var result = await _mediator.Send(query);

            return Ok(result.Server);
        }
    }
}