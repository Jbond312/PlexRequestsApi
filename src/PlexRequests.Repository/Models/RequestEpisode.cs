using System;
using PlexRequests.Store.Enums;

namespace PlexRequests.Store.Models
{
    public class RequestEpisode
    {
        public string Title { get; set; }
        public int Index { get; set; }
        public string PlexMediaUri { get; set; }
        public RequestStatuses Status { get; set; }
        public string ImagePath { get; set; }
        public DateTime? AirDate { get; set; }
    }
}