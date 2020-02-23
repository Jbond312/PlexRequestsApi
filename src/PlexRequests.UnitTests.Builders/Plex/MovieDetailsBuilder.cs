using System;
using PlexRequests.TheMovieDb.Models;

namespace PlexRequests.UnitTests.Builders.Plex
{
    public class MovieDetailsBuilder : IBuilder<MovieDetails>
    {
        private int _id;
        private string _backDropPath;
        private string _imdbId;
        private string _posterPath;
        private string _releaseDate;
        private string _status;
        private string _title;

        public MovieDetailsBuilder()
        {
            _id = new Random().Next(1, int.MaxValue);
            _backDropPath = Guid.NewGuid().ToString();
            _imdbId = Guid.NewGuid().ToString();
            _posterPath = Guid.NewGuid().ToString();
            _releaseDate = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
            _status = Guid.NewGuid().ToString();
            _title = Guid.NewGuid().ToString();
        }

        public MovieDetailsBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        public MovieDetailsBuilder WithReleaseDate(string date)
        {
            _releaseDate = date;
            return this;
        }

        public MovieDetails Build()
        {
            return new MovieDetails
            {
                Id = _id,
                Backdrop_Path = _backDropPath,
                Imdb_Id = _imdbId,
                Poster_Path = _posterPath,
                Release_Date = _releaseDate,
                Status = _status,
                Title = _title
            };
        }
    }
}
