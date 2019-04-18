using System;
using PlexRequests.Repository.Enums;

namespace PlexRequests.Repository.Models
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