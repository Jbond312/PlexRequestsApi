using System;

namespace PlexRequests.Store.Models
{
    public class RequestEpisode
    {
        public string Title { get; set; }
        public int Episode { get; set; }
        public bool IsApproved { get; set; }
        public string ImagePath { get; set; }
        public DateTime? AirDate { get; set; }
    }
}