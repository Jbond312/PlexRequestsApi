using PlexRequests.DataAccess.Enums;

namespace PlexRequests.Functions.Features.Search.Models
{
    public class EpisodeModel
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public string Overview { get; set; }
        public int SeasonIndex { get; set; }
        public string StillPath { get; set; }
        public RequestStatuses? RequestStatus { get; set; }
        public string PlexMediaUri { get; set; }
    }
}
