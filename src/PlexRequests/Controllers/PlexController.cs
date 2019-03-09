using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PlexRequests.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlexController : Controller
    {
        [HttpGet]
        [Route("")]
        public async Task<List<string>> Get()
        {
            return new List<string> {"value1", "value2"};
        }
    }
}
