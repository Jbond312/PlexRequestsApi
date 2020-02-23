using System;
using System.Collections.Generic;
using System.Linq;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.UnitTests.Builders.DataAccess
{
    public class TvPlexMediaItemRowBuilder : IBuilder<PlexMediaItemRow>
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
        private List<PlexSeasonRowBuilder> _plexSeasonBuilders;

        public TvPlexMediaItemRowBuilder()
        {
            _plexMediaItemId = new Random().Next(1, int.MaxValue);
            _identifier= Guid.NewGuid();
            _key = new Random().Next(1, int.MaxValue);
            _title = Guid.NewGuid().ToString();
            _agentType = AgentTypes.TheMovieDb;
            _agentSourceId = "447109";
            _mediaUri = Guid.NewGuid().ToString();
            _year = DateTime.UtcNow.Year;
            _mediaType = PlexMediaTypes.Show;
            _resolution = Guid.NewGuid().ToString();
            _plexSeasonBuilders = new List<PlexSeasonRowBuilder>();

            WithPlexSeasons();
        }

        public TvPlexMediaItemRowBuilder WithId(int id)
        {
            _plexMediaItemId = id;
            return this;
        }

        public TvPlexMediaItemRowBuilder WithIdentifier(Guid identifier)
        {
            _identifier = identifier;
            return this;
        }

        public TvPlexMediaItemRowBuilder WithKey(int key)
        {
            _key = key;
            return this;
        }

        public TvPlexMediaItemRowBuilder WithAgentType(AgentTypes agentType)
        {
            _agentType = agentType;
            return this;
        }

        public TvPlexMediaItemRowBuilder WithAgentSource(string agentSourceId)
        {
            _agentSourceId = agentSourceId;
            return this;
        }

        public TvPlexMediaItemRowBuilder WithPlexSeasons(int count = 3)
        {
            for (var i = 0; i < count; i++)
            {
                _plexSeasonBuilders.Add(new PlexSeasonRowBuilder().WithId(i));
            }

            return this;
        }
        public TvPlexMediaItemRowBuilder WithPlexSeason(PlexSeasonRowBuilder plexSeasonBuilder)
        {
            _plexSeasonBuilders.Add(plexSeasonBuilder);
            return this;
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
                MediaType = _mediaType,
                PlexSeasons = _plexSeasonBuilders.Select(x => x.Build()).ToList()
            };
        }
    }
}
