using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlexRequests.Plex;
using PlexRequests.Plex.Models.OAuth;

namespace PlexRequests.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlexController : Controller
    {
        private readonly IPlexApi _plexApi;

        public PlexController(IPlexApi plexApi)
        {
            _plexApi = plexApi;
        }

        [HttpGet]
        [Route("")]
        public async Task<List<string>> Get()
        {
            return new List<string> {"value1", "value2"};
        }

        [HttpPost]
        [Route("Pins")]
        public async Task<OAuthPin> CreatePin()
        {
            return await _plexApi.CreatePin();
        }
    }
}
