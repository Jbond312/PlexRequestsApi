﻿// ReSharper disable InconsistentNaming
namespace PlexRequests.TheMovieDb.Models
{
    public class BelongsToCollection
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Poster_Path { get; set; }
        public string Backdrop_Path { get; set; }
    }
}