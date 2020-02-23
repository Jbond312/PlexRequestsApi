using System;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.UnitTests.Builders.DataAccess
{
    public class MoviePlexMediaItemRowBuilder : IBuilder<PlexMediaItemRow>
    {
        private int _plexMediaItemId;
        private Guid _identifier;
        private int _key;
        private string _title;
        private AgentTypes _agentType;
        private string _agentSourceId;
        private string _mediaUri;
        private int? _year;
        private PlexMediaTypes _mediaType;
        private string _resolution;

        public MoviePlexMediaItemRowBuilder()
        {
            _plexMediaItemId = new Random().Next(1, int.MaxValue);
            _identifier= Guid.NewGuid();
            _key = new Random().Next(1, int.MaxValue);
            _title = Guid.NewGuid().ToString();
            _agentType = AgentTypes.TheMovieDb;
            _agentSourceId = "447109";
            _mediaUri = Guid.NewGuid().ToString();
            _year = DateTime.UtcNow.Year;
            _mediaType = PlexMediaTypes.Movie;
            _resolution = Guid.NewGuid().ToString();
        }

        public PlexMediaItemRow Build()
        {
            return new PlexMediaItemRow
            {
                PlexMediaItemId = _plexMediaItemId,
                Identifier = _identifier,
                MediaItemKey = _key,
                Title = _title,
                AgentType = _agentType,
                AgentSourceId = _agentSourceId,
                MediaUri = _mediaUri,
                Year = _year,
                Resolution = _resolution,
                MediaType = _mediaType
            };
        }
    }
}
