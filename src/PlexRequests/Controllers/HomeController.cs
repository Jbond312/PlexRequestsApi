using Microsoft.AspNetCore.Mvc;

namespace PlexRequests.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Home()
        {
            return Ok();
        }
    }
}
