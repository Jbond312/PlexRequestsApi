using System;

namespace PlexRequests.ApiRequests.TheMovieDb.DTOs
{
    public class MovieSearchModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public string PosterPath { get; set; }
        public string BackdropPath { get; set; }
        public DateTime? ReleaseDate { get; set; }
    }
}
