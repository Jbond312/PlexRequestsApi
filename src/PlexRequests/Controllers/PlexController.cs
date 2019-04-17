using System.Collections.Generic;
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

        [HttpPost("SyncUsers")]
        [Admin]
        public async Task<IActionResult> SyncUsers()
        {
            await _mediator.Send(new SyncUsersCommand());

            return Ok();
        }

        [HttpPost]
        [Route("SyncLibraries")]
        [Admin]
        public async Task<ActionResult<List<PlexServerLibraryDetailModel>>> SyncLibraries()
        {
            var result = await _mediator.Send(new SyncLibrariesCommand());

            return Ok(result.Libraries);
        }

        [HttpPost]
        [Route("SyncContent")]
        [Admin]
        public async Task<ActionResult> SyncContent([FromQuery] bool fullRefresh = false)
        {
            var command = new SyncContentCommand(fullRefresh);

            await _mediator.Send(command);

            return Ok();
        }

        [HttpGet]
        [Route("Server")]
        [Admin]
        public async Task<ActionResult<PlexServerDetailModel>> GetServer()
        {
            var query = new GetServerQuery();

            var result = await _mediator.Send(query);

            return Ok(result.Server);
        }
    }
}