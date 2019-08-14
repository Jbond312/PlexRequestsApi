using System;

namespace PlexRequests.ApiRequests.TheMovieDb.Models
{
    public class EpisodeToAirModel
    {
        public int Id { get; set; }
        public DateTime? AirDate { get; set; }
        public int Season { get; set; }
        public int Episode { get; set; }
        public string StillPath { get; set; }
        public string Name { get; set; }
    }
}
