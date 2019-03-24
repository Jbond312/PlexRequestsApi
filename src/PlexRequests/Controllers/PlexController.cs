using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
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

        [HttpPost]
        [Route("Pins")]
        public async Task<OAuthPin> CreatePin()
        {
            return await _plexApi.CreatePin();
        }

        [HttpGet]
        [Route("Pins/{pinId:int}")]
        public async Task<OAuthPin> CreatePin([FromRoute] int pinId)
        {
            return await _plexApi.GetPin(pinId);
        }

        [HttpPost]
        [Route("SignIn")]
        public async Task<User> SignIn(string username, string password)
        {
            return await _plexApi.SignIn(username, password);
        }
    }
}
