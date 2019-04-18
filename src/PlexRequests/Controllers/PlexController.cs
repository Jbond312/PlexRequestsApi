using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlexRequests.Attributes;
using PlexRequests.Models.Plex;
using PlexRequests.Models.SubModels.Detail;

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
        public async Task<IActionResult> SyncUsers()
        {
            await _mediator.Send(new SyncUsersCommand());

            return Ok();
        }

        [HttpGet]
        [Route("Libraries")]
        [Admin]
        public async Task<ActionResult<GetManyPlexServerLibraryQueryResult>> GetLibraries()
        {
            var query = new GetManyPlexServerLibraryQuery();

            var libraries = await _mediator.Send(query);
            
            return Ok(libraries);
        }
        
        [HttpPut]
        [Route("Libraries/{key}")]
        [Admin]
        public async Task<ActionResult> UpdateLibrary(string key, UpdatePlexServerLibraryCommand command)
        {
            command.Key = key;

            var libraries = await _mediator.Send(command);
            
            return Ok(libraries);
        }
        
        [HttpPost("Libraries/Sync")]
        [Admin]
        public async Task<ActionResult> SyncLibraries()
        {
            await _mediator.Send(new SyncLibrariesCommand());

            return Ok();
        }

        [HttpPost("Content/Sync")]
        [Admin]
        public async Task<ActionResult> SyncContent([FromQuery] bool fullRefresh = false)
        {
            var command = new SyncContentCommand(fullRefresh);

            await _mediator.Send(command);

            return Ok();
        }

        [HttpGet("Server")]
        [Admin]
        public async Task<ActionResult<PlexServerDetailModel>> GetServer()
        {
            var query = new GetServerQuery();

            var result = await _mediator.Send(query);

            return Ok(result.Server);
        }
    }
}