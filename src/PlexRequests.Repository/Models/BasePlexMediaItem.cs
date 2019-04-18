using PlexRequests.Store.Enums;

namespace PlexRequests.Store.Models
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
