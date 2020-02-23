using System;
using System.Collections.Generic;
using System.Linq;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.UnitTests.Builders.DataAccess
{
    public class PlexEpisodeRowBuilder : IBuilder<PlexEpisodeRow>
    {
        private int _plexEpisodeId;
        private Guid _identifier;
        private int _episode;
        private int _mediaItemKey;
        private string _title;
        private AgentTypes _agentType;
        private string _mediaUri;
        private int? _year;
        private string _resolution;
        private List<TvRequestEpisodeRowBuilder> _tvRequests;

        public PlexEpisodeRowBuilder()
        {
            _plexEpisodeId = new Random().Next(1, int.MaxValue);
            _identifier = Guid.NewGuid();
            _episode = 1;
            _mediaItemKey = new Random().Next(1, int.MaxValue);
            _title = Guid.NewGuid().ToString();
            _agentType = AgentTypes.TheMovieDb;
            _mediaUri = Guid.NewGuid().ToString();
            _year = DateTime.UtcNow.Year;
            _resolution = Guid.NewGuid().ToString();
            _tvRequests = new List<TvRequestEpisodeRowBuilder>();
        }

        public PlexEpisodeRowBuilder WithId(int id)
        {
            _plexEpisodeId = id;
            return this;
        }

        public PlexEpisodeRowBuilder WithIdentifier(Guid identifier)
        {
            _identifier = identifier;
            return this;
        }

        public PlexEpisodeRowBuilder WithEpisode(int episode)
        {
            _episode = episode;
            return this;
        }

        public PlexEpisodeRowBuilder WithAgentType(AgentTypes agentType)
        {
            _agentType = agentType;
            return this;
        }

        public PlexEpisodeRowBuilder WithTvRequest(TvRequestEpisodeRowBuilder episodeBuilder)
        {
            _tvRequests.Add(episodeBuilder);
            return this;
        }

        public PlexEpisodeRow Build()
        {
            return new PlexEpisodeRow
            {
                PlexEpisodeId = _plexEpisodeId,
                Identifier = _identifier,
                Episode = _episode,
                MediaItemKey = _mediaItemKey,
                Title = _title,
                AgentType = _agentType,
                MediaUri = _mediaUri,
                Year = _year,
                Resolution = _resolution,
                TvRequestEpisodes = _tvRequests.Select(x => x.Build()).ToList()
            };
        }
    }
}
