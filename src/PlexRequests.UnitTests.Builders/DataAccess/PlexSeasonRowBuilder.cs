using System;
using System.Collections.Generic;
using System.Linq;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.UnitTests.Builders.DataAccess
{
    public class PlexSeasonRowBuilder : IBuilder<PlexSeasonRow>
    {
        private int _plexSeasonId;
        private Guid _identifier;
        private int _season;
        private int _mediaItemKey;
        private string _title;
        private AgentTypes _agentType;
        private string _agentSourceId;
        private string _mediaUri;
        private List<PlexEpisodeRowBuilder> _plexEpisodeBuilders;
        private List<TvRequestSeasonRowBuilder> _tvRequests;

        public PlexSeasonRowBuilder()
        {
            _plexSeasonId = new Random().Next(1, int.MaxValue);
            _identifier = Guid.NewGuid();
            _season = 1;
            _mediaItemKey = new Random().Next(1, int.MaxValue);
            _title = Guid.NewGuid().ToString();
            _agentType = AgentTypes.TheMovieDb;
            _agentSourceId = "446021";
            _mediaUri = Guid.NewGuid().ToString();
            _plexEpisodeBuilders = new List<PlexEpisodeRowBuilder>();
            _tvRequests = new List<TvRequestSeasonRowBuilder>();

            WithPlexEpisodes();
        }

        public PlexSeasonRowBuilder WithId(int id)
        {
            _plexSeasonId = id;
            return this;
        }

        public PlexSeasonRowBuilder WithIdentifier(Guid identifier)
        {
            _identifier = identifier;
            return this;
        }

        public PlexSeasonRowBuilder WithSeason(int season)
        {
            _season = season;
            return this;
        }

        public PlexSeasonRowBuilder WithAgentType(AgentTypes agentType)
        {
            _agentType = agentType;
            return this;
        }

        public PlexSeasonRowBuilder WithPlexEpisodes(int count = 3)
        {
            for (var i = 0; i < count; i++)
            {
                _plexEpisodeBuilders.Add(new PlexEpisodeRowBuilder().WithId(i).WithEpisode(i));
            }

            return this;
        }

        public PlexSeasonRowBuilder WithPlexEpisode(PlexEpisodeRowBuilder episodeRowBuilder)
        {
            _plexEpisodeBuilders.Add(episodeRowBuilder);
            return this;
        }

        public PlexSeasonRowBuilder WithTvRequest(TvRequestSeasonRowBuilder episodeBuilder)
        {
            _tvRequests.Add(episodeBuilder);
            return this;
        }

        public PlexSeasonRow Build()
        {
            return new PlexSeasonRow
            {
                PlexSeasonId = _plexSeasonId,
                Identifier = _identifier,
                Season = _season,
                MediaItemKey = _mediaItemKey,
                Title = _title,
                AgentType = _agentType,
                AgentSourceId = _agentSourceId,
                MediaUri = _mediaUri,
                PlexEpisodes = _plexEpisodeBuilders.Select(x => x.Build()).ToList(),
                TvRequestSeasons = _tvRequests.Select(x => x.Build()).ToList()
            };
        }
    }
}
