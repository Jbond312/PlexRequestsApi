using System;

namespace PlexRequests.ApiRequests.Health.Models
{
    public class HealthModel
    {
        public string ApplicationName { get; set; }
        public string Version { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
