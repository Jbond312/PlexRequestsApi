using PlexRequests.Repository.Enums;

namespace PlexRequests.Repository.Models
{
    public class BasePlexMediaItem
    {
        public int Key { get; set; }
        public string Title { get; set; }
        public AgentTypes AgentType { get; set; }
        public string AgentSourceId { get; set; }
        public string PlexMediaUri { get; set; }
    }
}
