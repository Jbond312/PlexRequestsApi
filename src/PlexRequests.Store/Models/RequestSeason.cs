using System;
using System.Collections.Generic;

namespace PlexRequests.Store.Models
{
    public class RequestSeason
    {
        public int Season { get; set; }
        public List<RequestEpisode> Episodes { get; set; }
        public string ImagePath { get; set; }
        public DateTime? AirDate { get; set; }
    }
}