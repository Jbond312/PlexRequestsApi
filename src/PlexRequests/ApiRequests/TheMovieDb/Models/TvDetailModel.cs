using System;
using System.Collections.Generic;

namespace PlexRequests.ApiRequests.TheMovieDb.Models
{
    public class TvDetailModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public string PosterPath { get; set; }
        public string BackdropPath { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int? EpisodeCount { get; set; }
        public int? SeasonCount { get; set; }
        public bool InProduction { get; set; }
        public string Status { get; set; }
        public List<string> Networks { get; set; }
        public EpisodeToAirModel LastEpisode { get; set; }
        public EpisodeToAirModel NextEpisode { get; set; }
    }
}
