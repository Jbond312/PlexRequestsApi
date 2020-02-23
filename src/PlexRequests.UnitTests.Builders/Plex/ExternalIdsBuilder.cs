using System;
using PlexRequests.TheMovieDb.Models;

namespace PlexRequests.UnitTests.Builders.Plex
{
    public class ExternalIdsBuilder : IBuilder<ExternalIds>
    {
        private string _imdbId;
        private string _tvDbId;

        public ExternalIdsBuilder()
        {
            _imdbId = Guid.NewGuid().ToString();
            _tvDbId = Guid.NewGuid().ToString();
        }

        public ExternalIds Build()
        {
            return new ExternalIds
            {
                Imdb_Id = _imdbId,
                TvDb_Id = _tvDbId
            };
        }
    }
}
