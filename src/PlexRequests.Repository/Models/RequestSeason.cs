using System;
using System.Collections.Generic;

namespace PlexRequests.Repository.Models
{
    public class RequestSeason
    {
        public int Index { get; set; }
        public string PlexMediaUri { get; set; }
        public List<RequestEpisode> Episodes { get; set; }
        public string ImagePath { get; set; }
        public DateTime? AirDate { get; set; }
    }
}