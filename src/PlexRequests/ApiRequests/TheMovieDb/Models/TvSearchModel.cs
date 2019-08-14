using System;

namespace PlexRequests.ApiRequests.TheMovieDb.Models
{
    public class TvSearchModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public string PosterPath { get; set; }
        public string BackdropPath { get; set; }
        public DateTime? ReleaseDate { get; set; }
    }
}
