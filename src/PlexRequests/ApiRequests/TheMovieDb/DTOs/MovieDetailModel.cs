using System;

namespace PlexRequests.ApiRequests.TheMovieDb.DTOs
{
    public class MovieDetailModel
    {
        public int Id { get; set; }
        public string ImdbId { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public string PosterPath { get; set; }
        public string BackdropPath { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int? Runtime { get; set; }
    }
}
