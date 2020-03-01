using System;

namespace PlexRequests.Functions.Features.Health.Models
{
    public class HealthModel
    {
        public string ApplicationName { get; set; }
        public string Version { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
