using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PlexRequests.ApiRequests.Users.Queries;
using PlexRequests.Core.Settings;
using Swashbuckle.AspNetCore.Annotations;

namespace PlexRequests.Controllers
{
    [Route("api/[controller]")]
    public class HealthController : Controller
    {
        private readonly PlexRequestsSettings _settings;

        public HealthController(IOptions<PlexRequestsSettings> plexRequestOptions)
        {
            _settings = plexRequestOptions.Value;
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(200)]
        public ActionResult HealthCheck([FromQuery] GetManyUserQuery query)
        {
            var data = new
            {
                _settings.Version,
                _settings.ApplicationName,
                Time = DateTime.UtcNow
            };

            return Ok(data);
        }
    }
}
